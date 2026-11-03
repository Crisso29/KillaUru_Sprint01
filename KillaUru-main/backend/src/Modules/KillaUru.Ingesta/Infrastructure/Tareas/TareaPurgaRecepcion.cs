using KillaUru.Ingesta.Application.Tareas;
using KillaUru.Shared.Tareas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KillaUru.Ingesta.Infrastructure.Tareas;

/// <summary>Calendario de PurgarRecepcion: al arrancar y cada día a las 03:00 Lima (FR-018, T-OPS05).</summary>
internal sealed class TareaPurgaRecepcion(
    IServiceScopeFactory scopes, TimeProvider reloj, ILogger<TareaPurgaRecepcion> logger)
    : TareaIngesta(scopes, reloj, logger)
{
    protected override DateTimeOffset Proxima(DateTimeOffset ahora) =>
        ProgramaReloj.ProximaDiaria(ahora, new TimeOnly(3, 0));
    protected override Task EjecutarAsync(IServiceProvider servicios, CancellationToken ct) =>
        servicios.GetRequiredService<PurgarRecepcionUseCase>().EjecutarAsync(ct);
}
