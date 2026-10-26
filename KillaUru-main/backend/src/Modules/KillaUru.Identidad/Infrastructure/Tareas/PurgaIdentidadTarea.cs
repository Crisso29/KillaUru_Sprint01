using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Tareas;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace KillaUru.Identidad.Infrastructure.Tareas;

/// <summary>Ejecuta al arrancar y a las 03:45 de Lima; un fallo no detiene la API y deja de emitir éxito.</summary>
internal sealed class PurgaIdentidadTarea(IServiceScopeFactory scopes, TimeProvider reloj,
    IOptions<IdentidadOptions> opciones, ILogger<PurgaIdentidadTarea> logger) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> Fallo = LoggerMessage.Define(LogLevel.Error,
        new EventId(3003, "PurgaIdentidadFallida"), "Falló purga-identidad; revisar disponibilidad de la base de datos.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await EjecutarPurgaAsync(stoppingToken);
            var proxima = ProgramaReloj.ProximaDiaria(reloj.GetUtcNow(), opciones.Value.HoraPurgaIdentidad);
            await Task.Delay(proxima - reloj.GetUtcNow(), reloj, stoppingToken);
        }
    }

    private async Task EjecutarPurgaAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<PurgarIdentidadUseCase>().EjecutarAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception) { Fallo(logger, null); }
    }
}
