using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
namespace KillaUru.Auditoria.Application;

/// <summary>Valida y consulta; no entrega resultados si no pudo confirmar el rastro de acceso.</summary>
internal sealed class ConsultarAuditoriaUseCase(IConsultaAuditoria consulta, RegistrarLecturaUseCase registrar)
{
    internal async Task<PaginaAuditoria?> EjecutarAsync(FiltrosAuditoria filtros, Guid actorId, string rol, CancellationToken ct)
    {
        if (!filtros.EsValido) return null;
        var pagina = await consulta.ConsultarAsync(filtros, ct);
        // Se registra después de leer: la consulta actual aparece a partir de la siguiente página solicitada.
        await registrar.EjecutarAsync(AccionAuditada.AccesoAdministrativoDatos, new(actorId, rol),
            "auditoria", new Dictionary<string, object?> { ["filtros"] = filtros }, ct);
        return pagina;
    }
}
