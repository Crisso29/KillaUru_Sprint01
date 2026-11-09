using KillaUru.Ingesta.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>Proyección para EF Core de la tabla evento_recibido.</summary>
internal sealed class EventoRecibidoFila
{
    public Guid EventoId { get; set; }
    public Guid? LoteId { get; set; }
    public DateTimeOffset RecibidoUtc { get; set; }
    public DateTimeOffset DetectadoUtc { get; set; }
    public Guid? Seudonimo { get; set; }
    public bool ConSesion { get; set; }
    public EstadoRecepcion Estado { get; set; }
    public short? Motivo { get; set; }
    public string Payload { get; set; } = "{}";
}
