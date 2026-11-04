using Microsoft.EntityFrameworkCore;

namespace KillaUru.Dashboard.Infrastructure;

internal sealed class DashboardDbContext(DbContextOptions<DashboardDbContext> options) : DbContext(options)
{
    internal DbSet<IndicadorDiario> IndicadoresDiarios => Set<IndicadorDiario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new IndicadorDiarioConfig());
}
