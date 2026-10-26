using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace KillaUru.Anomalias.Infrastructure.Tareas;
/// <summary>Calendario de CaducarAnomalias; la ejecución inicial se conserva tras un reinicio.</summary>
internal sealed class TareaCaducidad(IServiceScopeFactory scopes, TimeProvider reloj, ILogger<TareaCaducidad> logger)
    : TareaAnomalias(scopes, reloj, logger)
{
    protected override DateTimeOffset Proxima(DateTimeOffset ahora) => ProgramaReloj.ProximaHoraria(ahora, 5);
    protected override Task EjecutarAsync(IServiceProvider servicios, CancellationToken ct) =>
        servicios.GetRequiredService<CaducarAnomaliasUseCase>().EjecutarAsync(ct);
}
