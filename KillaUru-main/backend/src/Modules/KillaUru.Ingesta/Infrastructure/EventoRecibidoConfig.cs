using KillaUru.Ingesta.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>Mapeo EF Core de la tabla ingesta.evento_recibido (data-model §1).</summary>
internal sealed class EventoRecibidoConfig : IEntityTypeConfiguration<EventoRecibidoFila>
{
    public void Configure(EntityTypeBuilder<EventoRecibidoFila> b)
    {
        b.ToTable("evento_recibido");
        b.HasKey(f => f.EventoId);
        b.Property(f => f.EventoId).HasColumnName("evento_id");
        b.Property(f => f.LoteId).HasColumnName("lote_id");
        b.Property(f => f.RecibidoUtc).HasColumnName("recibido_utc");
        b.Property(f => f.DetectadoUtc).HasColumnName("detectado_utc");
        b.Property(f => f.Seudonimo).HasColumnName("seudonimo");
        b.Property(f => f.ConSesion).HasColumnName("con_sesion");
        b.Property(f => f.Estado).HasColumnName("estado").HasConversion<short>();
        b.Property(f => f.Motivo).HasColumnName("motivo");
        b.Property(f => f.Payload).HasColumnName("payload").HasColumnType("jsonb");
    }
}

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
