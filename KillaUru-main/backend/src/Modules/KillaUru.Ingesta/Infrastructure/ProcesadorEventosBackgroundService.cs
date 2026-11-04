using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>
/// Lanza N trabajadores concurrentes que leen la cola y procesan cada lote con su propio scope
/// (FR-012; N = Trabajadores o número de núcleos si Trabajadores = 0).
/// </summary>
internal sealed class ProcesadorEventosBackgroundService(
    IColaLotes cola,
    IServiceScopeFactory scopes,
    IOptions<Application.IngestaOptions> opciones,
    ILogger<ProcesadorEventosBackgroundService> logger) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> ErrorProcesando =
        LoggerMessage.Define(LogLevel.Warning, new EventId(5001, "ErrorProcesandoLote"),
            "Error al procesar lote de Ingesta; la fila sigue Recibido para el barredor.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int n = opciones.Value.Trabajadores > 0
            ? opciones.Value.Trabajadores
            : Environment.ProcessorCount;

        var trabajadores = Enumerable.Range(0, n)
            .Select(_ => TrabajadorAsync(stoppingToken))
            .ToArray();

        await Task.WhenAll(trabajadores);
    }

    private async Task TrabajadorAsync(CancellationToken ct)
    {
        await foreach (var aviso in cola.LeerAsync(ct))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var uc = scope.ServiceProvider.GetRequiredService<Application.Procesamiento.ProcesarLoteUseCase>();
                await uc.EjecutarAsync(aviso.LoteId, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception ex) { ErrorProcesando(logger, ex); }
        }
    }
}
