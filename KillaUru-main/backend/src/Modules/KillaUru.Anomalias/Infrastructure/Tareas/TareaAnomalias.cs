using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace KillaUru.Anomalias.Infrastructure.Tareas;
/// <summary>Ejecuta al arrancar y por reloj; cada pasada tiene scope y los fallos no producen métricas de éxito.</summary>
internal abstract class TareaAnomalias(IServiceScopeFactory scopes, TimeProvider reloj, ILogger logger) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> Fallo = LoggerMessage.Define(LogLevel.Error,
        new EventId(4001, "TareaAnomaliasFallida"), "Falló una tarea de Anomalías; revisar disponibilidad de la base de datos.");
    protected abstract DateTimeOffset Proxima(DateTimeOffset ahora);
    protected abstract Task EjecutarAsync(IServiceProvider servicios, CancellationToken ct);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await IntentarAsync(stoppingToken);
            var ahora = reloj.GetUtcNow();
            // Task.Delay no admite demoras negativas: si ya es la hora, la siguiente pasada corre enseguida.
            var demora = Proxima(ahora) - ahora;
            await Task.Delay(demora > TimeSpan.Zero ? demora : TimeSpan.Zero, reloj, stoppingToken);
        }
    }
    private async Task IntentarAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await EjecutarAsync(scope.ServiceProvider, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception) { Fallo(logger, null); }
    }
}
