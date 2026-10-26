using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Se evalúa dentro de la transacción serializada para impedir retirar al último administrador.</summary>
internal sealed class ProteccionAdministradores(IUsuarioRepositorio usuarios)
{
    internal async Task<string?> ValidarRetiroAsync(Usuario usuario, Guid actor, CancellationToken ct)
    {
        if (usuario.Id == actor) return "accion_sobre_si_mismo";
        if (usuario.Rol == RolUsuario.Administrador && usuario.Activo && (await usuarios.AdministradoresAsync(ct)).Count <= 1)
            return "ultimo_administrador";
        return null;
    }
}
