using KillaUru.Ingesta.Application.Procesamiento;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>
/// Barredor de pendientes: encola lotes al arrancar (sin filtro) y cada IntervaloBarredorSegundos
/// con filtro de AntiguedadMinimaBarredorSegundos (FR-016, EQ-09).
/// </summary>
internal sealed class BarredorBackgroundService(
    IServiceScopeFactory scopes,
    TimeProvider reloj,
    IOptions<Application.IngestaOptions> opciones,
    ILogger<BarredorBackgroundService> logger) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> Error =
        LoggerMessage.Define(LogLevel.Warning, new EventId(5002, "ErrorBarredorIngesta"),
            "Error en el barredor de Ingesta; se reintentará en el próximo ciclo.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Al arrancar: encola todo sin filtro de antigüedad
        await IntentarAsync(null, stoppingToken);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(opciones.Value.IntervaloBarredorSegundos), reloj);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var anteriorA = reloj.GetUtcNow()
                .AddSeconds(-opciones.Value.AntiguedadMinimaBarredorSegundos);
            await IntentarAsync(anteriorA, stoppingToken);
        }
    }

    private async Task IntentarAsync(DateTimeOffset? anteriorA, CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var uc = scope.ServiceProvider.GetRequiredService<BarrerPendientesUseCase>();
            await uc.EjecutarAsync(anteriorA, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex) { Error(logger, ex); }
    }
}
