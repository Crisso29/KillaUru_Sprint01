namespace KillaUru.Ingesta.Adapters.Dtos;

/// <summary>Respuesta 200 de POST /api/v1/anomalies/batch (FR-006, D5).</summary>
internal sealed class RespuestaLoteDto
{
    /// <summary>UUIDv7 generado por el servidor para este lote (D3).</summary>
    public Guid LoteId { get; init; }

    /// <summary>EventoIds de los eventos insertados en este envío.</summary>
    public IReadOnlyList<Guid> Aceptados { get; init; } = [];

    /// <summary>EventoIds que ya existían en la base de datos.</summary>
    public IReadOnlyList<Guid> Duplicados { get; init; } = [];

    /// <summary>Eventos que no se guardaron, con el motivo y su posición 0-based en el arreglo original.</summary>
    public IReadOnlyList<RechazoDto> Rechazados { get; init; } = [];
}
