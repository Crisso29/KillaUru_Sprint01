using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Identidad.Infrastructure;

/// <summary>Único contexto dueño del esquema identidad; no consulta tablas de otros módulos.</summary>
internal sealed class IdentidadDbContext(DbContextOptions<IdentidadDbContext> options) : DbContext(options)
{
    internal DbSet<Usuario> Usuarios => Set<Usuario>();
    internal DbSet<SesionRenovacion> Sesiones => Set<SesionRenovacion>();
    internal DbSet<Consentimiento> Consentimientos => Set<Consentimiento>();
    internal DbSet<TokenCorreo> Tokens => Set<TokenCorreo>();
    internal DbSet<RevisionPrivilegios> Revisiones => Set<RevisionPrivilegios>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identidad");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentidadDbContext).Assembly);
    }
}
