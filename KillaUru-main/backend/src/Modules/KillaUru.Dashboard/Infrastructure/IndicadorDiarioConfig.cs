using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Dashboard.Infrastructure;

internal sealed class IndicadorDiarioConfig : IEntityTypeConfiguration<IndicadorDiario>
{
    public void Configure(EntityTypeBuilder<IndicadorDiario> builder)
    {
        builder.ToTable("indicador_diario", "dashboard");
        builder.HasKey(x => new { x.Dia, x.Indicador });
        builder.Property(x => x.Dia).HasColumnType("date");
        builder.Property(x => x.Indicador).HasColumnType("smallint").HasConversion<short>();
        builder.Property(x => x.Valor).HasColumnType("numeric");
    }
}
