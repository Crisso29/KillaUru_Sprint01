using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Identidad.Infrastructure.Repositorios;

/// <summary>Consulta de última revisión sin cargar todo el historial.</summary>
internal sealed class RevisionRepositorio(IdentidadDbContext db) : IRevisionRepositorio
{
    public Task<DateTimeOffset?> UltimaAsync(CancellationToken ct) =>
        db.Revisiones.Select(x => (DateTimeOffset?)x.RealizadaUtc).MaxAsync(ct);
    public void Agregar(RevisionPrivilegios revision) => db.Revisiones.Add(revision);
}
