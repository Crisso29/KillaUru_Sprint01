using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Application.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Prepara administradores reales por invitación, sin depender de la semilla de uso único.</summary>
internal static class AdministradoresPrueba
{
    /// <summary>Invita a un administrador y canjea el enlace para fijar su contraseña.</summary>
    internal static async Task<string> CrearAsync(EscenarioIdentidad escenario)
    {
        var correo = EscenarioIdentidad.CorreoNuevo();
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var crear = scope.ServiceProvider.GetRequiredService<CrearUsuarioUseCase>();
        var resultado = await crear.EjecutarAsync(correo, "Administrador", new(Guid.NewGuid(), "Administrador"), EscenarioIdentidad.Ct);
        Assert.Null(resultado.Error);
        var fijar = await escenario.FijarContrasenaAsync(escenario.Correo.Token(correo), EscenarioIdentidad.Contrasena);
        Assert.Equal(HttpStatusCode.NoContent, fijar.StatusCode);
        return correo;
    }

    /// <summary>Inicia sesión por el canal de la app (token en el cuerpo) y deja el cliente autenticado.</summary>
    internal static async Task<JsonElement> AutenticarAppAsync(EscenarioIdentidad escenario, string correo)
    {
        var respuesta = await escenario.IniciarSesionAsync(correo);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var sesion = await respuesta.Content.ReadFromJsonAsync<JsonElement>(EscenarioIdentidad.Ct);
        escenario.Autenticar(sesion);
        return sesion;
    }

    /// <summary>Crea un administrador y deja el cliente autenticado como él.</summary>
    internal static async Task<string> CrearYAutenticarAsync(EscenarioIdentidad escenario)
    {
        var correo = await CrearAsync(escenario);
        await AutenticarAppAsync(escenario, correo);
        return correo;
    }
}
