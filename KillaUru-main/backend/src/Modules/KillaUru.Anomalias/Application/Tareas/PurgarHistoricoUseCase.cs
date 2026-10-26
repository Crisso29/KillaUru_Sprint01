using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Anomalias.Application.Tareas;

/// <summary>
/// RN-10: borra los reportes y las anomalías cerradas con más de 2 años, por lotes hasta agotarlos,
/// y solo entonces registra el éxito (FR-022).
/// </summary>
internal sealed class PurgarHistoricoUseCase(IPurgasRepository purgas, TimeProvider reloj, IOptions<AnomaliasOptions> opciones, IMetricas metricas)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        var limite = reloj.GetUtcNow().AddDays(-opciones.Value.RetencionHistoricaDias);
        while (LoteMantenimiento.QuedanPendientes(await purgas.PurgarHistoricoAsync(limite, ct))) ct.ThrowIfCancellationRequested();
        metricas.RegistrarTareaCorrecta("purga-historica");
    }
}
