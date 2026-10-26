using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace KillaUru.Anomalias.Infrastructure.Tareas;
/// <summary>Calendario de PurgarHistorico; la ejecución inicial se conserva tras un reinicio.</summary>
internal sealed class TareaPurgaHistorica(IServiceScopeFactory scopes, TimeProvider reloj, ILogger<TareaPurgaHistorica> logger)
    : TareaAnomalias(scopes, reloj, logger)
{
    protected override DateTimeOffset Proxima(DateTimeOffset ahora) => ProgramaReloj.ProximaDiaria(ahora, new TimeOnly(3, 30));
    protected override Task EjecutarAsync(IServiceProvider servicios, CancellationToken ct) =>
        servicios.GetRequiredService<PurgarHistoricoUseCase>().EjecutarAsync(ct);
}
