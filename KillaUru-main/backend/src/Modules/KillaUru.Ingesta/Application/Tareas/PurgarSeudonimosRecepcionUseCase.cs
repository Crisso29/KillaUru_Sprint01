using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Application.Tareas;

/// <summary>
/// Pone seudonimo=NULL, lote_id=NULL y redondea recibido_utc a la hora para filas antiguas.
/// Corre al arrancar y cada hora en el minuto :10 (FR-017, RN-09; T-OPS05).
/// </summary>
internal sealed class PurgarSeudonimosRecepcionUseCase(
    IPurgasRecepcion purgas, TimeProvider reloj,
    IOptions<IngestaOptions> opciones, IMetricas metricas)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        // Se purga con 1 h de anticipación para que ningún seudónimo supere las 72 h entre pasadas.
        var limite = reloj.GetUtcNow().AddHours(-(opciones.Value.PurgaSeudonimoHoras - 1));
        int afectadas;
        do
        {
            afectadas = await purgas.PurgarSeudonimosAsync(limite, ct);
            ct.ThrowIfCancellationRequested();
        }
        while (afectadas == 5000);
        metricas.RegistrarTareaCorrecta("purga-seudonimos-recepcion");
    }
}
