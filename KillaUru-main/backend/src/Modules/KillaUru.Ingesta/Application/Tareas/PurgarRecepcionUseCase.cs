using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Application.Tareas;

/// <summary>
/// Borra las filas Procesado/Descartado con más de RetencionRecepcionDias días.
/// Corre al arrancar y cada día a las 03:00 hora local (FR-018, RN-10; T-OPS05).
/// </summary>
internal sealed class PurgarRecepcionUseCase(
    IPurgasRecepcion purgas, TimeProvider reloj,
    IOptions<IngestaOptions> opciones, IMetricas metricas)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        var limite = reloj.GetUtcNow().AddDays(-opciones.Value.RetencionRecepcionDias);
        int afectadas;
        do
        {
            afectadas = await purgas.BorrarFinalesAsync(limite, ct);
            ct.ThrowIfCancellationRequested();
        }
        while (afectadas == 5000);
        metricas.RegistrarTareaCorrecta("purga-recepcion");
    }
}
