using Microsoft.EntityFrameworkCore;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>Contexto dedicado al esquema ingesta; solo usado por RecepcionEventosPostgres y el repositorio de pendientes.</summary>
internal sealed class IngestaDbContext(DbContextOptions<IngestaDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ingesta");
        modelBuilder.ApplyConfiguration(new EventoRecibidoConfig());
    }
}
