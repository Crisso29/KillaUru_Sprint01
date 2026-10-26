using KillaUru.Identidad.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
namespace KillaUru.Identidad.Application.Tareas;

/// <summary>La señal de éxito se emite después de completar todos los lotes de retención.</summary>
internal sealed class PurgarIdentidadUseCase(IPurgaIdentidadRepositorio repositorio, IMetricas metricas,
    IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        await repositorio.PurgarAsync(reloj.GetUtcNow(), opciones.Value.RetencionIdentidadDias, opciones.Value.TruncarUsoTrasMinutos, ct);
        metricas.RegistrarTareaCorrecta("purga-identidad");
    }
}
