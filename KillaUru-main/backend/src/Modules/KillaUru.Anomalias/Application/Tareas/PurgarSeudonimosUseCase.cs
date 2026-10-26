using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Anomalias.Application.Tareas;

/// <summary>Procesa lotes hasta agotarlos y solo entonces registra el éxito (FR-021).</summary>
internal sealed class PurgarSeudonimosUseCase(IPurgasRepository purgas, TimeProvider reloj, IOptions<AnomaliasOptions> opciones, IMetricas metricas)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        // La tarea corre cada hora: se purga con una hora de anticipación para que ningún seudónimo
        // llegue a superar las 72 h entre dos ejecuciones.
        var limite = reloj.GetUtcNow().AddHours(-(opciones.Value.PurgaSeudonimoHoras - 1));
        while (LoteMantenimiento.QuedanPendientes(await purgas.PurgarSeudonimosAsync(limite, ct))) ct.ThrowIfCancellationRequested();
        metricas.RegistrarTareaCorrecta("purga-seudonimos");
    }
}
