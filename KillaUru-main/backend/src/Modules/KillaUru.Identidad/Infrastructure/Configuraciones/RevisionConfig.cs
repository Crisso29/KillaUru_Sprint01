using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Identidad.Infrastructure.Configuraciones;

/// <summary>Mapeo explícito de revision_privilegios; la migración versionada define índices y restricciones.</summary>
internal sealed class RevisionConfig : IEntityTypeConfiguration<RevisionPrivilegios>
{
    public void Configure(EntityTypeBuilder<RevisionPrivilegios> builder)
    {
        builder.ToTable("revision_privilegios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.RealizadaUtc).HasColumnName("realizada_utc");
        builder.Property(x => x.ActorId).HasColumnName("actor_id");
        builder.Property(x => x.Confirmados).HasColumnName("confirmados");
        builder.Property(x => x.Retirados).HasColumnName("retirados");

    }
}
