using System.Buffers.Binary;
using System.Security.Cryptography;
using KillaUru.Mapa.Application.Puertos;
using KillaUru.Mapa.Domain;

namespace KillaUru.Mapa.Application;

/// <summary>
/// Calcula el ETag de una respuesta por celdas como SHA-256(arranque, alcance, versiones).
/// Un ETag siempre tiene el mismo formato: comillas incluidas (spec 006 §CalculadorETag; ADR-006).
/// </summary>
internal sealed class CalculadorETag(IVersionesCelda versiones)
{
    /// <summary>
    /// Genera el ETag con los 16 caracteres hex más significativos del SHA-256.
    /// <paramref name="celdas"/> debe estar ordenada para que el ETag sea determinista.
    /// </summary>
    internal string Calcular(Alcance alcance, IReadOnlyList<long> celdas)
    {
        // 8 bytes arranque + 1 byte alcance + n×16 bytes (celda + versión)
        var buffer = new byte[9 + celdas.Count * 16];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, versiones.InstanteArranque.UtcTicks);
        buffer[8] = (byte)alcance;

        for (int i = 0; i < celdas.Count; i++)
        {
            var id = celdas[i];
            BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(9 + i * 16), id);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(9 + i * 16 + 8), versiones.ObtenerVersion(id));
        }

        var hash = SHA256.HashData(buffer);
        return $"\"{Convert.ToHexString(hash)[..16]}\"";
    }
}
