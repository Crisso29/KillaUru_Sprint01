using KillaUru.Dashboard.Application;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KillaUru.Dashboard.Infrastructure;

internal sealed class TareaIndicadoresDiarios : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly TimeProvider         _reloj;
    private readonly TimeOnly             _horaLocal;
    private readonly ILogger<TareaIndicadoresDiarios> _log;

    private static readonly Action<ILogger, Exception?> _logError =
        LoggerMessage.Define(LogLevel.Error, default, "Error calculando indicadores diarios");

    public TareaIndicadoresDiarios(IServiceScopeFactory scopes, TimeProvider reloj,
        IOptions<DashboardOptions> opciones, ILogger<TareaIndicadoresDiarios> log)
    {
        _scopes    = scopes;
        _reloj     = reloj;
        _horaLocal = opciones.Value.HoraCalculoLocal;
        _log       = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await IntentarAsync(stoppingToken);
            var ahora = _reloj.GetUtcNow();
            var demora = ProgramaReloj.ProximaDiaria(ahora, _horaLocal) - ahora;
            await Task.Delay(demora > TimeSpan.Zero ? demora : TimeSpan.Zero, _reloj, stoppingToken);
        }
    }

    private async Task IntentarAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<CalcularIndicadoresUseCase>().EjecutarAsync(ct);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { _logError(_log, ex); }
    }
}
