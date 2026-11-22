using Microsoft.AspNetCore.Http;

namespace KillaUru.Ingesta.Tests.Adapters;

/// <summary>
/// Doble del cuerpo HTTP: al leer lanza lo mismo que Kestrel cuando se supera su `MaxRequestBodySize`
/// (`BadHttpRequestException` con estado 413). Recuerda si alguien intentó leerlo.
/// </summary>
internal sealed class CuerpoCortadoPorElServidor : MemoryStream
{
    internal bool FueLeido { get; private set; }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        FueLeido = true;
        throw new BadHttpRequestException("Request body too large.", StatusCodes.Status413PayloadTooLarge);
    }
}
