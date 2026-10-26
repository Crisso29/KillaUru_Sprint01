using KillaUru.Identidad.Application.Resultados;
namespace KillaUru.Identidad.Application.Usuarios;

/// <summary>Resultado de una invitación, con códigos de conflicto estables.</summary>
internal sealed record ResultadoUsuario(UsuarioVista? Usuario = null, string? Error = null, int Estado = 201);
