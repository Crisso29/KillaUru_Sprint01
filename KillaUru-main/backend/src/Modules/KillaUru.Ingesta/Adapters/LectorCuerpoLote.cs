using System.Text.Json;
using KillaUru.Ingesta.Adapters.Dtos;
using Microsoft.AspNetCore.Http;

namespace KillaUru.Ingesta.Adapters;

/// <summary>
/// Lee el cuerpo de POST /api/v1/anomalies/batch con un tope de bytes configurable (`Ingesta:TamanoMaximoCuerpoKb`,
/// R-002). Nunca guarda en memoria más que el tope más un bloque, y distingue el 413 del 400 del contrato 005.
/// Sustituye a `[FromBody]` porque el modelo de MVC respondía `datos_invalidos` y el corte de Kestrel acababa en 500.
/// </summary>
internal static class LectorCuerpoLote
{
    private const int TamanoBloque = 8 * 1024;

    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    /// <param name="cuerpo">Cuerpo de la petición.</param>
    /// <param name="longitudDeclarada">Cabecera `Content-Length`; null si el cliente envía por bloques.</param>
    /// <param name="topeBytes">Máximo de bytes aceptados (inclusivo).</param>
    /// <param name="ct">Cancelación de la petición.</param>
    internal static async Task<LecturaLote> LeerAsync(Stream cuerpo, long? longitudDeclarada, long topeBytes, CancellationToken ct)
    {
        // Si el tamaño ya se declara mayor, se responde 413 sin leer el cuerpo (spec 005, casos borde; R-002).
        if (longitudDeclarada > topeBytes) return LecturaLote.DemasiadoGrande;
        byte[]? bytes;
        try
        {
            bytes = await CopiarConTopeAsync(cuerpo, topeBytes, ct);
        }
        catch (BadHttpRequestException e) when (e.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            // Kestrel corta antes que nosotros si su propio límite es menor: también es un lote demasiado grande.
            return LecturaLote.DemasiadoGrande;
        }

        return bytes is null ? LecturaLote.DemasiadoGrande : Interpretar(bytes);
    }

    /// <summary>Copia hasta <paramref name="topeBytes"/>; null en cuanto llega un byte de más.</summary>
    private static async Task<byte[]?> CopiarConTopeAsync(Stream cuerpo, long topeBytes, CancellationToken ct)
    {
        using var destino = new MemoryStream();
        var bloque = new byte[TamanoBloque];
        int leidos;
        while ((leidos = await cuerpo.ReadAsync(bloque, ct)) > 0)
        {
            if (destino.Length + leidos > topeBytes) return null;
            destino.Write(bloque, 0, leidos);
        }

        return destino.ToArray();
    }

    private static LecturaLote Interpretar(byte[] bytes)
    {
        try
        {
            var lote = JsonSerializer.Deserialize<LoteDto>(bytes, Opciones);
            return lote?.Eventos is { } eventos ? LecturaLote.Leido(eventos) : LecturaLote.Invalido;
        }
        catch (JsonException)
        {
            return LecturaLote.Invalido;
        }
    }
}
