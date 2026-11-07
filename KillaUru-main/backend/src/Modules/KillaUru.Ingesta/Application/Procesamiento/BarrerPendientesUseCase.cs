using KillaUru.Ingesta.Application.Puertos;
namespace KillaUru.Ingesta.Application.Procesamiento;
/// <summary>
/// Encola todos los lotes con filas Recibido; respeta la capacidad de la cola y procesa
/// en lotes de 100 loteIds para no cargar la BD de golpe (FR-016; R-011).
/// </summary>
internal sealed class BarrerPendientesUseCase(
    IBarridoPendientes barrido,
    IColaLotes cola)
{
    private const int TamanoPagina = 100;
    /// <summary>Encola los lotes pendientes. <paramref name="anteriorA"/> null = todos (al arrancar).</summary>
    internal async Task EjecutarAsync(DateTimeOffset? anteriorA, CancellationToken ct)
    {
        Guid? desde = null;
        bool continuar = true;
        while (continuar)
        {
            var lotes = await barrido.LotesPendientesAsync(desde, TamanoPagina, ct);
            if (lotes.Count == 0) break;
            foreach (var loteId in lotes)
            {
                if (!cola.TryEscribir(new LoteRecibido(loteId)))
                {
                    continuar = false;
                    break;
                }
            }
            desde = lotes.LastOrDefault(id => id.HasValue);
            if (lotes.Count < TamanoPagina) break;
        }
    }
}
