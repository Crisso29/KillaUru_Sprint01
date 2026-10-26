using KillaUru.Anomalias.Application.Administracion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Auditoria.Contracts;
namespace KillaUru.Anomalias.Application.Exportacion;
/// <summary>Audita antes del primer byte; el conteo y el stream usan una instantánea consistente.</summary>
internal sealed class ExportarDatasetUseCase(IExportacionRepository repo, IAuditoriaService auditoria, IEnumerable<IEscritorExportacion> escritores)
{
    internal async Task EjecutarAsync(SolicitudExportacion solicitud, ActorAnomalias actor, Stream destino, CancellationToken ct)
    {
        var escritor = escritores.Single(e => e.Formato == solicitud.Formato);
        await using var instantanea = await repo.AbrirInstantaneaAsync(ct);
        var cantidad = await repo.ContarAsync(solicitud, ct);
        // La lectura queda auditada antes del primer byte: sin rastro no sale ningún dato (FR-028).
        await auditoria.RegistrarLecturaAsync(AccionAuditada.DatasetExportado, new(actor.Id, actor.Rol), "anomalias",
            ContextoAuditoria(solicitud, cantidad), ct);
        await escritor.EscribirAsync(destino, repo.LeerAsync(solicitud, ct), ct);
    }

    /// <summary>Qué se exportó y cuántas filas, sin ningún dato de las anomalías mismas.</summary>
    private static Dictionary<string, object?> ContextoAuditoria(SolicitudExportacion solicitud, int cantidad) => new()
    {
        ["formato"] = solicitud.Formato, ["filas"] = cantidad, ["desdeUtc"] = solicitud.DesdeUtc,
        ["hastaUtc"] = solicitud.HastaUtc, ["tipo"] = solicitud.Tipo, ["estados"] = solicitud.Estados,
    };
}
