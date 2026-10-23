using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KillaUru.Auditoria.Infrastructure;

/// <summary>Mapeo explícito al SQL versionado; EF no crea ni modifica el esquema al arrancar.</summary>
internal sealed class RegistroAuditoriaConfiguracion : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("registro");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(r => r.OcurridoUtc).HasColumnName("ocurrido_utc");
        builder.Property(r => r.ActorId).HasColumnName("actor_id");
        builder.Property(r => r.ActorRol).HasColumnName("actor_rol").HasConversion(
            rol => rol == Roles.Administrador ? (short)2 : (short)1,
            codigo => codigo == 2 ? Roles.Administrador : Roles.ConductorRegistrado);
        builder.Property(r => r.Accion).HasColumnName("accion").HasColumnType("smallint");
        builder.Property(r => r.Objetivo).HasColumnName("objetivo").HasMaxLength(100);
        builder.Property(r => r.Contexto).HasColumnName("contexto").HasColumnType("jsonb");
    }
}
