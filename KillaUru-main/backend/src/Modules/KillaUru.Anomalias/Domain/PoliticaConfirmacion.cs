using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Selecciona vehículos mutuamente independientes; no cuenta seudónimos ya purgados.</summary>
internal sealed class PoliticaConfirmacion(AnomaliasOptions opciones)
{
    internal EstadoAnomalia Decidir(IReadOnlyList<ReporteAnomalia> ventana, Severidad mostrada, bool porTope, DateTimeOffset ultima)
    {
        var desde = ultima.AddHours(-opciones.VentanaConfirmacionHoras);
        var recientes = ventana.Where(r => r.DetectadoUtc >= desde && r.DetectadoUtc <= ultima).ToArray();
        var grave = mostrada.Valor >= opciones.SeveridadAltaDesde || recientes.Any(r => r.Severidad.Valor >= opciones.SeveridadAltaDesde);
        // Una leve que no se creó por superar el tope se publica sin esperar a otro vehículo (FR-010).
        if (!grave && !porTope) return EstadoAnomalia.Activa;
        var conSeudonimo = recientes.Where(r => r.Seudonimo is not null).OrderBy(r => r.DetectadoUtc).ToArray();
        // La lluvia de un solo dispositivo no requiere explorar pares que nunca podrán confirmar.
        var seudonimosDistintos = conSeudonimo.Select(r => r.Seudonimo).Distinct().Take(opciones.DispositivosRequeridos).Count();
        if (seudonimosDistintos < opciones.DispositivosRequeridos) return EstadoAnomalia.PendienteConfirmacion;
        return Buscar(conSeudonimo, [], 0, grave) ? EstadoAnomalia.Activa : EstadoAnomalia.PendienteConfirmacion;
    }

    internal bool EsVehiculoNuevo(ReporteAnomalia reporte, IReadOnlyList<ReporteAnomalia> ventana) =>
        reporte.Seudonimo is not null && ventana.Where(x => x.EventoId != reporte.EventoId && x.Seudonimo is not null)
            .All(x => Distintos(x, reporte));

    /// <summary>
    /// Vuelta atrás: busca un grupo de vehículos mutuamente independientes (seudónimo distinto y
    /// ≥ 3 min entre sí); si es grave, al menos uno debe venir con sesión (FR-011).
    /// </summary>
    private bool Buscar(ReporteAnomalia[] candidatos, List<ReporteAnomalia> elegidos, int inicio, bool grave)
    {
        if (elegidos.Count >= opciones.DispositivosRequeridos)
            return !grave || elegidos.Count(r => r.ConSesion) >= opciones.ReportesConSesionRequeridos;
        for (var i = inicio; i < candidatos.Length; i++)
        {
            if (!elegidos.All(e => Distintos(e, candidatos[i]))) continue;
            elegidos.Add(candidatos[i]);
            if (Buscar(candidatos, elegidos, i + 1, grave)) return true;
            elegidos.RemoveAt(elegidos.Count - 1);
        }
        return false;
    }

    /// <summary>
    /// Dos reportes son de vehículos distintos con seudónimos distintos y detecciones separadas ≥ 3 min:
    /// así dos celulares en el mismo auto cuentan como uno.
    /// </summary>
    private bool Distintos(ReporteAnomalia a, ReporteAnomalia b)
    {
        var seudonimosDistintos = a.Seudonimo is not null && b.Seudonimo is not null && a.Seudonimo != b.Seudonimo;
        var separadosEnElTiempo = (a.DetectadoUtc - b.DetectadoUtc).Duration() >= TimeSpan.FromMinutes(opciones.SeparacionMinimaMinutos);
        return seudonimosDistintos && separadosEnElTiempo;
    }
}
