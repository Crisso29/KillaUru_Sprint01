using System.Data;
using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Domain;
using Microsoft.EntityFrameworkCore;
namespace KillaUru.Auditoria.Infrastructure;

/// <summary>Consulta indexada con proyección; COUNT y página comparten una instantánea consistente.</summary>
internal sealed class ConsultaAuditoriaRepositorio(FabricaContextoAuditoria fabrica) : IConsultaAuditoria
{
    public async Task<PaginaAuditoria> ConsultarAsync(FiltrosAuditoria filtros, CancellationToken ct)
    {
        await using var db = fabrica.CrearPropio();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var consulta = Filtrar(db.Registros.AsNoTracking(), filtros);
        var total = await consulta.LongCountAsync(ct);
        var elementos = await consulta.OrderByDescending(r => r.OcurridoUtc).ThenByDescending(r => r.Id)
            .Skip((filtros.Pagina - 1) * filtros.Tamano).Take(filtros.Tamano)
            .Select(r => new RegistroConsultado(r.Id, r.OcurridoUtc, r.ActorId, r.ActorRol, r.Accion, r.Objetivo, r.Contexto))
            .ToListAsync(ct);
        await tx.CommitAsync(ct);
        return new(elementos, filtros.Pagina, filtros.Tamano, total);
    }

    private static IQueryable<RegistroAuditoria> Filtrar(IQueryable<RegistroAuditoria> consulta, FiltrosAuditoria filtros)
    {
        if (filtros.Desde is { } desde) consulta = consulta.Where(r => r.OcurridoUtc >= desde.ToUniversalTime());
        if (filtros.Hasta is { } hasta) consulta = consulta.Where(r => r.OcurridoUtc <= hasta.ToUniversalTime());
        if (filtros.ActorId is { } actor) consulta = consulta.Where(r => r.ActorId == actor);
        if (filtros.Categoria is not null || filtros.Accion is not null)
        {
            var acciones = filtros.Acciones;
            consulta = consulta.Where(r => acciones.Contains(r.Accion));
        }
        return consulta;
    }
}
