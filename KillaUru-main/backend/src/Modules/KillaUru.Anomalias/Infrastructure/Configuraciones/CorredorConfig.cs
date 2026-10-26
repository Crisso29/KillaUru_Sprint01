using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KillaUru.Anomalias.Infrastructure.Configuraciones;
/// <summary>Una traza LineString WGS84 versionada por migración de datos.</summary>
internal sealed class CorredorConfig : IEntityTypeConfiguration<CorredorFila>
{
    public void Configure(EntityTypeBuilder<CorredorFila> b)
    {
        b.ToTable("corredor_via"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Traza).HasColumnName("traza").HasColumnType("geography(LineString,4326)");
    }
}
