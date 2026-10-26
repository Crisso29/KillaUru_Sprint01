using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
namespace KillaUru.Anomalias.Application.Tareas;
/// <summary>Procesa lotes hasta agotarlos; anticipa una hora la anonimización para respetar el máximo de 72 h.</summary>
internal sealed class PurgarHistoricoUseCase(IPurgasRepository repo, TimeProvider reloj, IOptions<AnomaliasOptions> opciones, IMetricas metricas)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        var limite = reloj.GetUtcNow().AddDays(-opciones.Value.RetencionHistoricaDias);
        while (await repo.PurgarHistoricoAsync(limite, ct) == 5000) ct.ThrowIfCancellationRequested();
        metricas.RegistrarTareaCorrecta("purga-historica");
    }
}
