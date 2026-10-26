using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Identidad.Infrastructure.Configuraciones;

/// <summary>Mapeo explícito de sesion_refresh; la migración versionada define índices y restricciones.</summary>
internal sealed class SesionConfig : IEntityTypeConfiguration<SesionRenovacion>
{
    public void Configure(EntityTypeBuilder<SesionRenovacion> builder)
    {
        builder.ToTable("sesion_refresh");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.FamiliaId).HasColumnName("familia_id");
        builder.Property(x => x.HashToken).HasColumnName("hash_token");
        builder.Property(x => x.CreadoUtc).HasColumnName("creado_utc");
        builder.Property(x => x.ExpiraUtc).HasColumnName("expira_utc");
        builder.Property(x => x.LimiteAbsolutoUtc).HasColumnName("limite_absoluto_utc");
        builder.Property(x => x.UsadoUtc).HasColumnName("usado_utc");
        builder.Property(x => x.RevocadoUtc).HasColumnName("revocado_utc");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
