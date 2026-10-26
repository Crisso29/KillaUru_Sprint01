using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>No devuelve datos personales si no logra registrar su lectura.</summary>
internal sealed class ListarUsuariosUseCase(IUsuarioRepositorio usuarios, AuditarIdentidad auditoria)
{
    internal async Task<PaginaUsuarios?> EjecutarAsync(FiltrosUsuarios filtros, ActorIdentidad actor, CancellationToken ct)
    {
        if (!filtros.Validos) return null;
        var pagina = await usuarios.ListarAsync(filtros, ct);
        await auditoria.LecturaAsync(actor, "identidad.usuarios", ct);
        return pagina;
    }
}
