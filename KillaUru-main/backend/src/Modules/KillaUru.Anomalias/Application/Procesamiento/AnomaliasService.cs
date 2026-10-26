using KillaUru.Anomalias.Contracts;
namespace KillaUru.Anomalias.Application.Procesamiento;
/// <summary>Adaptador del contrato público; las reglas permanecen en el dominio.</summary>
internal sealed class AnomaliasService(EvaluarIntrinsecaUseCase evaluar, ProcesarReporteUseCase procesar) : IAnomaliasService
{
    public MotivoDescarte? EvaluarIntrinseca(EventoReportado evento, DateTimeOffset recibidoUtc) => evaluar.Ejecutar(evento, recibidoUtc);
    public Task<ResultadoProceso> ProcesarReporteAsync(EventoReportado evento, ContextoReporte contexto, CancellationToken ct) =>
        procesar.EjecutarAsync(evento, contexto, ct);
}
