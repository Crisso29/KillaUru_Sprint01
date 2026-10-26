using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using Microsoft.Extensions.Options;
using MotivoDescarte = KillaUru.Anomalias.Contracts.MotivoDescarte;
namespace KillaUru.Anomalias.Application.Procesamiento;
/// <summary>Adapta la entrada de Ingesta sin acceder a persistencia ni convertir errores en excepciones.</summary>
internal sealed class EvaluarIntrinsecaUseCase(EstadoCorredor estado, IOptions<AnomaliasOptions> opciones)
{
    internal MotivoDescarte? Ejecutar(EventoReportado evento, DateTimeOffset recibidoUtc)
    {
        var (reporte, motivo) = MapearReporte.Desde(evento, new(false, 0, recibidoUtc));
        if (motivo is not null) return (MotivoDescarte)motivo;
        var corredor = estado.Actual;
        return corredor is null ? MotivoDescarte.FueraDelCorredor
            : (MotivoDescarte?)new ReglasCalidad(opciones.Value, corredor).EvaluarIntrinseca(reporte!, recibidoUtc);
    }
}
