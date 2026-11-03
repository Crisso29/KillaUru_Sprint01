using KillaUru.Ingesta.Application.Tareas;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KillaUru.Ingesta.Infrastructure.Tareas;

/// <summary>Calendario de PurgarSeudonimosRecepcion: al arrancar y cada hora en el minuto :10 (FR-017, T-OPS05).</summary>
internal sealed class TareaPurgaSeudonimosRecepcion(
    IServiceScopeFactory scopes, TimeProvider reloj, ILogger<TareaPurgaSeudonimosRecepcion> logger)
    : TareaIngesta(scopes, reloj, logger)
{
    protected override DateTimeOffset Proxima(DateTimeOffset ahora) => ProgramaReloj.ProximaHoraria(ahora, 10);
    protected override Task EjecutarAsync(IServiceProvider servicios, CancellationToken ct) =>
        servicios.GetRequiredService<PurgarSeudonimosRecepcionUseCase>().EjecutarAsync(ct);
}
