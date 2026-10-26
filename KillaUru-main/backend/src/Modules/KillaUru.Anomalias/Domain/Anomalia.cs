using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;
/// <summary>Agregado con posición fija y ciclo de vida RN-07.</summary>
internal sealed class Anomalia
{
    public Guid Id { get; private set; }
    public TipoAnomalia Tipo { get; private set; }
    public Coordenada Posicion { get; private set; }
    public Celda Celda { get; private set; }
    public EstadoAnomalia Estado { get; private set; }
    public Severidad SeveridadMostrada { get; private set; }
    public EstadoPersistencia Persistencia { get; private set; }
    public DateTimeOffset UltimaDeteccionUtc { get; private set; }
    public DateTimeOffset VenceUtc { get; private set; }
    public DateTimeOffset EstadoDesdeUtc { get; private set; }
    public DateTimeOffset ActualizadaUtc { get; private set; }
    public float KmVia { get; private set; }
    public bool CreadaPorTope { get; private set; }
    public bool PorRevisar { get; private set; }
    public List<DateTimeOffset> MarcasRevision { get; private set; } = [];
    internal List<Transicion> Transiciones { get; } = [];
    private bool nueva;

    internal static Anomalia Crear(ReporteAnomalia reporte, bool porTope, DateTimeOffset ahora) => new()
    {
        Id = Guid.CreateVersion7(ahora), Tipo = reporte.Tipo, Posicion = reporte.Posicion,
        Celda = Celda.De(reporte.Posicion), KmVia = reporte.KmVia, CreadaPorTope = porTope,
        UltimaDeteccionUtc = reporte.DetectadoUtc, SeveridadMostrada = reporte.Severidad,
        Estado = EstadoAnomalia.PendienteConfirmacion, EstadoDesdeUtc = reporte.DetectadoUtc, nueva = true,
    };

    internal void RegistrarReporte(ReporteAnomalia reporte, IReadOnlyList<ReporteAnomalia> ventana, AnomaliasOptions opciones, DateTimeOffset ahora)
    {
        EstadoAnomalia? antes = nueva ? null : Estado;
        var renace = ReglaRenacimiento.Aplica(Estado, reporte.DetectadoUtc, VenceUtc, EstadoDesdeUtc);
        UltimaDeteccionUtc = reporte.DetectadoUtc > UltimaDeteccionUtc ? reporte.DetectadoUtc : UltimaDeteccionUtc;
        ActualizadaUtc = ahora;
        ActualizarResumen(ventana, opciones);
        // Una Descartada nunca se reactiva sola: solo acumula vehículos para «Por revisar» (FR-020).
        if (Estado == EstadoAnomalia.Descartada) Revisar(reporte, ventana, opciones);
        else if (renace || EnCurso) ActualizarCicloDeVida(renace, ventana, opciones);
        Anotar(antes, UltimaDeteccionUtc, null);
    }

    private bool EnCurso => Estado is EstadoAnomalia.Activa or EstadoAnomalia.PendienteConfirmacion;

    /// <summary>
    /// Se decide la confirmación al nacer, al renacer o mientras espera; una Activa no vuelve a
    /// pendiente (FR-012). El vencimiento se recalcula con cada reporte (FR-017).
    /// </summary>
    private void ActualizarCicloDeVida(bool renace, IReadOnlyList<ReporteAnomalia> ventana, AnomaliasOptions opciones)
    {
        var debeDecidir = renace || Estado == EstadoAnomalia.PendienteConfirmacion;
        if (debeDecidir) Estado = new PoliticaConfirmacion(opciones).Decidir(ventana, SeveridadMostrada, CreadaPorTope, UltimaDeteccionUtc);
        VenceUtc = UltimaDeteccionUtc + ReglasAnomalia.Caducidad(Estado, Persistencia, opciones);
    }

    internal bool Moderar(AccionAdmin accion, DateTimeOffset ahora, AnomaliasOptions opciones, Guid actor)
    {
        var destino = TablaModeracion.Destino(Estado, accion, PorRevisar);
        if (destino is null) return false;
        var antes = Estado;
        Estado = destino.Value;
        // Reabrir le da un plazo nuevo desde ahora; las demás acciones dejan el vencimiento como estaba.
        if (accion == AccionAdmin.Reabrir) VenceUtc = ahora + ReglasAnomalia.Caducidad(Estado, Persistencia, opciones);
        // Toda acción del administrador cierra la revisión pendiente (FR-026).
        MarcasRevision.Clear();
        PorRevisar = false;
        ActualizadaUtc = ahora;
        Anotar(antes, ahora, actor);
        return true;
    }

    /// <summary>Pasa a Expirada si venció; la transición se fecha en el vencimiento, no en la hora de la tarea (FR-018).</summary>
    internal void Caducar(DateTimeOffset ahora)
    {
        var vencida = EnCurso && VenceUtc <= ahora;
        if (!vencida) return;
        var antes = Estado;
        Estado = EstadoAnomalia.Expirada;
        ActualizadaUtc = ahora;
        Anotar(antes, VenceUtc, null);
    }

    private void ActualizarResumen(IReadOnlyList<ReporteAnomalia> ventana, AnomaliasOptions opciones)
    {
        var recientes = ventana.Where(r => r.DetectadoUtc >= UltimaDeteccionUtc.AddDays(-opciones.VentanaSeveridadDias)
            && r.DetectadoUtc <= UltimaDeteccionUtc).ToArray();
        if (recientes.Length > 0) SeveridadMostrada = Severidad.MedianaHaciaArriba(recientes.Select(r => r.Severidad));
        Persistencia = new ReglaPersistencia(opciones).Evaluar(ventana.Select(r => r.DetectadoUtc), UltimaDeteccionUtc);
    }

    private void Revisar(ReporteAnomalia reporte, IReadOnlyList<ReporteAnomalia> ventana, AnomaliasOptions opciones)
    {
        var desde = UltimaDeteccionUtc.AddDays(-opciones.VentanaRevisionDescartadaDias);
        var recientes = ventana.Where(r => r.DetectadoUtc >= desde && r.DetectadoUtc <= UltimaDeteccionUtc).ToArray();
        // Solo cuentan vehículos nuevos que la vieron después de descartarla y dentro de los 7 días.
        var dentroDeLaVentana = reporte.DetectadoUtc >= desde;
        var posteriorAlDescarte = reporte.DetectadoUtc > EstadoDesdeUtc;
        var esOtroVehiculo = new PoliticaConfirmacion(opciones).EsVehiculoNuevo(reporte, recientes);
        if (dentroDeLaVentana && posteriorAlDescarte && esOtroVehiculo) MarcasRevision.Add(reporte.DetectadoUtc);
        MarcasRevision.RemoveAll(m => m < desde);
        PorRevisar = MarcasRevision.Count >= opciones.VehiculosRevisionDescartada;
    }

    private void Anotar(EstadoAnomalia? antes, DateTimeOffset momento, Guid? actor)
    {
        if (antes != Estado)
        {
            EstadoDesdeUtc = momento;
            Transiciones.Add(new(antes, Estado, momento, actor));
        }
        nueva = false;
    }
}
