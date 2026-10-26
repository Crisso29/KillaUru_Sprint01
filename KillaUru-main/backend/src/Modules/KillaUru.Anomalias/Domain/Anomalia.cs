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
        if (Estado == EstadoAnomalia.Descartada) { Revisar(reporte, ventana, opciones); }
        else if (renace || Estado is EstadoAnomalia.Activa or EstadoAnomalia.PendienteConfirmacion)
        {
            if (renace || Estado == EstadoAnomalia.PendienteConfirmacion)
                Estado = new PoliticaConfirmacion(opciones).Decidir(ventana, SeveridadMostrada, CreadaPorTope, UltimaDeteccionUtc);
            VenceUtc = UltimaDeteccionUtc + ReglasAnomalia.Caducidad(Estado, Persistencia, opciones);
        }
        Anotar(antes, UltimaDeteccionUtc, null);
    }

    internal bool Moderar(AccionAdmin accion, DateTimeOffset ahora, AnomaliasOptions opciones, Guid actor)
    {
        var destino = TablaModeracion.Destino(Estado, accion, PorRevisar);
        if (destino is null) return false;
        var antes = Estado;
        Estado = destino.Value;
        if (accion == AccionAdmin.Reabrir) VenceUtc = ahora + ReglasAnomalia.Caducidad(Estado, Persistencia, opciones);
        MarcasRevision.Clear(); PorRevisar = false;
        ActualizadaUtc = ahora;
        Anotar(antes, ahora, actor);
        return true;
    }

    internal void Caducar(DateTimeOffset ahora)
    {
        if (Estado is not (EstadoAnomalia.Activa or EstadoAnomalia.PendienteConfirmacion) || VenceUtc > ahora) return;
        var antes = Estado; Estado = EstadoAnomalia.Expirada;
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
        if (reporte.DetectadoUtc >= desde && reporte.DetectadoUtc > EstadoDesdeUtc
            && new PoliticaConfirmacion(opciones).EsVehiculoNuevo(reporte, recientes))
        { MarcasRevision.Add(reporte.DetectadoUtc); }
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
