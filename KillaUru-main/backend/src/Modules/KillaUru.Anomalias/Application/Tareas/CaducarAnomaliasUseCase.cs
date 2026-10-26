using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;

namespace KillaUru.Anomalias.Application.Tareas;

/// <summary>
/// Pasa a Expirada las anomalías vencidas por lotes; cada lote es su propia transacción y la
/// invalidación del mapa solo se publica tras confirmarlo (FR-014, FR-018).
/// </summary>
internal sealed class CaducarAnomaliasUseCase(ICaducidadRepository caducidad, ITransaccionAnomalias transaccion,
    IPublicadorEventos eventos, IMetricas metricas, TimeProvider reloj)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        int caducadas;
        do caducadas = await transaccion.EjecutarAsync([], null, null, () => CaducarLoteAsync(ct), ct);
        while (LoteMantenimiento.QuedanPendientes(caducadas));
        metricas.RegistrarTareaCorrecta("caducidad");
    }

    private async Task<int> CaducarLoteAsync(CancellationToken ct)
    {
        var cambios = await caducidad.CaducarAsync(reloj.GetUtcNow(), ct);
        foreach (var cambio in cambios) transaccion.AlConfirmar(() => eventos.PublicarAsync(cambio, CancellationToken.None));
        return cambios.Count;
    }
}
