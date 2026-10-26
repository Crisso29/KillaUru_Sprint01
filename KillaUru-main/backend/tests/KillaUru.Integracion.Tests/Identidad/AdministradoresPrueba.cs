using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Application.Usuarios;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Prepara invitaciones reales desde Application, sin depender de la semilla de uso único.</summary>
internal static class AdministradoresPrueba
{
    internal static async Task<string> CrearAsync(EscenarioIdentidad e)
    {
        var correo = Guid.NewGuid() + "@example.com";
        await using var scope = e.Api.Services.CreateAsyncScope();
        var crear = scope.ServiceProvider.GetRequiredService<CrearUsuarioUseCase>();
        var resultado = await crear.EjecutarAsync(correo, "Administrador", new(Guid.NewGuid(), "Administrador"), TestContext.Current.CancellationToken);
        Assert.Null(resultado.Error);
        var fijar = await e.Cliente.PostAsJsonAsync("/api/v1/auth/fijar-contrasena",
            new { token = e.Correo.Token(correo), nuevaContrasena = EscenarioIdentidad.Contrasena }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, fijar.StatusCode);
        return correo;
    }

    internal static async Task<JsonElement> AutenticarAppAsync(EscenarioIdentidad e, string correo)
    {
        var respuesta = await e.Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion",
            new { correo, contrasena = EscenarioIdentidad.Contrasena, canal = "app" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var sesion = await respuesta.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        e.Autenticar(sesion);
        return sesion;
    }
}
