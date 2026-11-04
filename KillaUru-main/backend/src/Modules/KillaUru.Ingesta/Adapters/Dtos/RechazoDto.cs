namespace KillaUru.Ingesta.Adapters.Dtos;

/// <summary>Evento que no se guardó; Id es null cuando faltó el eventoId en el cuerpo (D5, FR-006).</summary>
internal sealed class RechazoDto
{
    /// <summary>eventoId del evento rechazado, o null si no vino en el cuerpo.</summary>
    public Guid? Id { get; init; }

    /// <summary>Posición 0-based del evento en el arreglo recibido.</summary>
    public int Indice { get; init; }

    /// <summary>Nombre del motivo del catálogo MotivoDescarte.</summary>
    public string Motivo { get; init; } = string.Empty;
}
