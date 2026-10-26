using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Identidad.Adapters;

/// <summary>Consulta e invitación de cuentas reservadas a administradores.</summary>
[Route("api/v1/usuarios"), Authorize(Roles = "Administrador")]
internal sealed class UsuariosController(ListarUsuariosUseCase listar, CrearUsuarioUseCase crear) : IdentidadControllerBase
{
    /// <summary>Listar usuarios por correo, rol, estado o ids.</summary>
    /// <remarks>Rol: administrador. Cada lectura queda auditada antes de devolver datos.</remarks>
    [HttpGet(Name = "listarUsuarios"), ProducesResponseType(typeof(PaginaUsuarios), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 401), ProducesResponseType(typeof(ProblemDetails), 403)]
    public async Task<IActionResult> Listar([FromQuery] ConsultaUsuarios consulta, CancellationToken ct)
    {
        var filtros = consulta.Filtros();
        var pagina = filtros is null ? null : await listar.EjecutarAsync(filtros, Actor, ct);
        return pagina is null ? RespuestaIdentidad.Error("datos_invalidos", 400, HttpContext) : Ok(pagina);
    }

    /// <summary>Invitar un usuario con el rol indicado.</summary>
    /// <remarks>Rol: administrador. No se asigna contraseña hasta canjear la invitación.</remarks>
    [HttpPost(Name = "crearUsuario"), ProducesResponseType(typeof(UsuarioVista), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403), ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Crear(CrearUsuarioSolicitud solicitud, CancellationToken ct)
    {
        var resultado = await crear.EjecutarAsync(solicitud.Correo, solicitud.Rol, Actor, ct);
        if (resultado.Error is not null) return RespuestaIdentidad.Error(resultado.Error, resultado.Estado, HttpContext);
        return StatusCode(201, resultado.Usuario);
    }
}
