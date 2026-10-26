using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Proyección explícita compartida; ningún hash sale de Application.</summary>
internal static class ProyectarUsuario
{
    internal static UsuarioVista Vista(Usuario usuario) => new(usuario.Id, usuario.Correo.Valor, usuario.Rol.ToString(),
        usuario.Estado, usuario.CorreoVerificado, usuario.CreadoUtc, null);
}
