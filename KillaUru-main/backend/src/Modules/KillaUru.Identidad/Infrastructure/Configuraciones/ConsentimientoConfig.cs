using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Identidad.Infrastructure.Configuraciones;

/// <summary>Mapeo explícito de consentimiento; la migración versionada define índices y restricciones.</summary>
internal sealed class ConsentimientoConfig : IEntityTypeConfiguration<Consentimiento>
{
    public void Configure(EntityTypeBuilder<Consentimiento> builder)
    {
        builder.ToTable("consentimiento");
        builder.HasKey(x => new { x.UsuarioId, x.Documento, x.Version });
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.Documento).HasColumnName("documento").HasConversion<short>();
        builder.Property(x => x.Version).HasColumnName("version");
        builder.Property(x => x.AceptadoUtc).HasColumnName("aceptado_utc");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
