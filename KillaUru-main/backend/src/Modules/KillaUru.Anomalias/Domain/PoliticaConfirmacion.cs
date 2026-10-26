using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Selecciona vehículos mutuamente independientes; no cuenta seudónimos ya purgados.</summary>
internal sealed class PoliticaConfirmacion(AnomaliasOptions opciones)
{
    internal EstadoAnomalia Decidir(IReadOnlyList<ReporteAnomalia> ventana, Severidad mostrada, bool porTope, DateTimeOffset ultima)
    {
        var recientes = ventana.Where(r => r.DetectadoUtc >= ultima.AddHours(-opciones.VentanaConfirmacionHoras)
            && r.DetectadoUtc <= ultima).ToArray();
        var grave = mostrada.Valor >= opciones.SeveridadAltaDesde || recientes.Any(r => r.Severidad.Valor >= opciones.SeveridadAltaDesde);
        if (!grave && !porTope) return EstadoAnomalia.Activa;
        var validos = recientes.Where(r => r.Seudonimo is not null).OrderBy(r => r.DetectadoUtc).ToArray();
        // La lluvia de un solo dispositivo no requiere explorar pares que nunca podrán confirmar.
        if (validos.Select(r => r.Seudonimo).Distinct().Take(opciones.DispositivosRequeridos).Count() < opciones.DispositivosRequeridos)
            return EstadoAnomalia.PendienteConfirmacion;
        return Buscar(validos, [], 0, grave) ? EstadoAnomalia.Activa : EstadoAnomalia.PendienteConfirmacion;
    }

    internal bool EsVehiculoNuevo(ReporteAnomalia reporte, IReadOnlyList<ReporteAnomalia> ventana) =>
        reporte.Seudonimo is not null && ventana.Where(x => x.EventoId != reporte.EventoId && x.Seudonimo is not null)
            .All(x => Distintos(x, reporte));

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

    private bool Distintos(ReporteAnomalia a, ReporteAnomalia b) => a.Seudonimo is not null
        && b.Seudonimo is not null && a.Seudonimo != b.Seudonimo
        && (a.DetectadoUtc - b.DetectadoUtc).Duration() >= TimeSpan.FromMinutes(opciones.SeparacionMinimaMinutos);
}
