using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Legal;

/// <summary>Lectura sin datos personales; no necesita transacción ni auditoría.</summary>
internal sealed class ObtenerVersionesLegalesUseCase(IOptions<IdentidadOptions> opciones)
{
    internal Task<VersionesLegales> EjecutarAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new VersionesLegales(new(opciones.Value.VersionPolitica, opciones.Value.UrlPolitica),
            new(opciones.Value.VersionTerminos, opciones.Value.UrlTerminos)));
    }
}
