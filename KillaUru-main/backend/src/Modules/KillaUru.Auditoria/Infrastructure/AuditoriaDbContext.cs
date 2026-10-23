using KillaUru.Auditoria.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Auditoria.Infrastructure;

/// <summary>Contexto exclusivo del esquema de Auditoría; nunca expone tablas de otro módulo.</summary>
internal sealed class AuditoriaDbContext(DbContextOptions<AuditoriaDbContext> opciones) : DbContext(opciones)
{
    internal DbSet<RegistroAuditoria> Registros => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auditoria");
        modelBuilder.ApplyConfiguration(new RegistroAuditoriaConfiguracion());
    }
}
