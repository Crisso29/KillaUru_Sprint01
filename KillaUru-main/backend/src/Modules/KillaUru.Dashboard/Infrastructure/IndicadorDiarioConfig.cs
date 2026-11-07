using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Dashboard.Infrastructure;

internal sealed class IndicadorDiarioConfig : IEntityTypeConfiguration<IndicadorDiario>
{
    public void Configure(EntityTypeBuilder<IndicadorDiario> builder)
    {
        builder.ToTable("indicador_diario", "dashboard");
        builder.HasKey(x => new { x.Dia, x.Indicador });
        builder.Property(x => x.Dia).HasColumnName("dia").HasColumnType("date");
        builder.Property(x => x.Indicador).HasColumnName("indicador").HasColumnType("smallint").HasConversion<short>();
        builder.Property(x => x.Valor).HasColumnName("valor").HasColumnType("numeric");
    }
}
