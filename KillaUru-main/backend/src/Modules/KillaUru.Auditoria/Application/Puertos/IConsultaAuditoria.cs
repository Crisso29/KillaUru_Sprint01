namespace KillaUru.Auditoria.Application.Puertos;

/// <summary>Puerto de lectura separado para no acoplar el consumidor a operaciones de escritura (ISP).</summary>
internal interface IConsultaAuditoria
{
    Task<PaginaAuditoria> ConsultarAsync(FiltrosAuditoria filtros, CancellationToken ct);
}
