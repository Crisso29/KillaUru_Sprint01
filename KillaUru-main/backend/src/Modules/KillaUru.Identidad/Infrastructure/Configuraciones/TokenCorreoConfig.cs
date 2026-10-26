using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Identidad.Infrastructure.Configuraciones;

/// <summary>Mapeo explícito de token_correo; la migración versionada define índices y restricciones.</summary>
internal sealed class TokenCorreoConfig : IEntityTypeConfiguration<TokenCorreo>
{
    public void Configure(EntityTypeBuilder<TokenCorreo> builder)
    {
        builder.ToTable("token_correo");
        builder.HasKey(x => x.HashToken);
        builder.Property(x => x.HashToken).HasColumnName("hash_token");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.Proposito).HasColumnName("proposito").HasConversion<short>();
        builder.Property(x => x.ExpiraUtc).HasColumnName("expira_utc");
        builder.Property(x => x.UsadoUtc).HasColumnName("usado_utc");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
