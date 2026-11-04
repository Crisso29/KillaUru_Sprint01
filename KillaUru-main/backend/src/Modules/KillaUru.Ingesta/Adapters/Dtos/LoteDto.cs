using System.Text.Json;

namespace KillaUru.Ingesta.Adapters.Dtos;

/// <summary>
/// Cuerpo del POST /api/v1/anomalies/batch. No incluye loteId: el servidor lo genera (D3, FR-025).
/// Los elementos de Eventos se deserializan individualmente para tolerancia de tipos (FR-002).
/// </summary>
internal sealed class LoteDto
{
    /// <summary>Arreglo de 1 a MaxEventosPorLote eventos; cada elemento se deserializa por separado.</summary>
    public IReadOnlyList<JsonElement>? Eventos { get; init; }
}
