using KillaUru.Anomalias.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Contexto dueño del esquema anomalias; nunca escribe datos de otros módulos.</summary>
internal sealed class AnomaliasDbContext(DbContextOptions<AnomaliasDbContext> opciones) : DbContext(opciones)
{
    internal DbSet<Anomalia> Anomalias => Set<Anomalia>();
    internal DbSet<ReporteAnomalia> Reportes => Set<ReporteAnomalia>();
    internal DbSet<Transicion> Transiciones => Set<Transicion>();
    internal DbSet<CorredorFila> Corredores => Set<CorredorFila>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("anomalias");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnomaliasDbContext).Assembly);
    }
}
