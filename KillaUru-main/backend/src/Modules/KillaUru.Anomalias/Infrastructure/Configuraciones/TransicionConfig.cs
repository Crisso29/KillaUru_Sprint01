using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KillaUru.Anomalias.Infrastructure.Configuraciones;
/// <summary>Historial inmutable de cambios; actor administrativo sin FK a Identidad.</summary>
internal sealed class TransicionConfig : IEntityTypeConfiguration<Transicion>
{
    public void Configure(EntityTypeBuilder<Transicion> b)
    {
        b.ToTable("transicion"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.AnomaliaId).HasColumnName("anomalia_id");
        b.Property(x => x.Desde).HasColumnName("desde").HasConversion<short?>();
        b.Property(x => x.Hacia).HasColumnName("hacia").HasConversion<short>();
        b.Property(x => x.OcurridaUtc).HasColumnName("ocurrida_utc");
        b.Property(x => x.ActorId).HasColumnName("actor_id");
        b.HasOne<Anomalia>().WithMany().HasForeignKey(x => x.AnomaliaId).OnDelete(DeleteBehavior.Cascade);
    }
}
