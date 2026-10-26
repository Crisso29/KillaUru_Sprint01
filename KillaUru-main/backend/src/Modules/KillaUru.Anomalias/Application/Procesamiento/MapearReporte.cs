using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Application.Procesamiento;
/// <summary>Frontera explícita: Domain no conoce los contratos públicos de Ingesta.</summary>
internal static class MapearReporte
{
    internal static (ReporteAnomalia? Reporte, Domain.MotivoDescarte? Motivo) Desde(EventoReportado e, ContextoReporte c) =>
        ReporteAnomalia.Desde(new(e.EventoId, e.DetectadoUtc, e.Latitud, e.Longitud, e.PrecisionMetros,
            e.Tipo, e.Severidad, e.Confidence, e.ModeloVersion, e.Seudonimo), new(c.ConSesion, c.EventosEnLaHora, c.RecibidoUtc));
}
