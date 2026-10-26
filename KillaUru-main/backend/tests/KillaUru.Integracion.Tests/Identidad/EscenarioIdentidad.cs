using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Host real y PostgreSQL real; solo se sustituyen el reloj, el correo y el anti-bot para que las
/// pruebas sean reproducibles. Los helpers nombran cada llamada de la API por lo que hace.
/// </summary>
internal sealed class EscenarioIdentidad : IDisposable
{
    internal const string Contrasena = "una-clave-larga";
    internal const string AntiBotValido = "antibot-prueba";

    internal CorreoDePrueba Correo { get; } = new();
    internal FakeTimeProvider Reloj { get; } = new(TimeProvider.System.GetUtcNow());
    internal WebApplicationFactory<Program> Api { get; }
    internal HttpClient Cliente { get; }
    internal static CancellationToken Ct => TestContext.Current.CancellationToken;

    internal EscenarioIdentidad(BaseDatosPrueba bd)
    {
        Api = new FabricaApi(bd).WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Identidad:ClaveRotacion", "clave-de-rotacion-solo-para-pruebas-003");
            builder.ConfigureTestServices(s =>
            {
                s.AddSingleton<TimeProvider>(Reloj);
                s.AddSingleton<IEnviadorCorreo>(Correo);
                s.AddSingleton<IVerificadorAntiBot, AntiBotDePrueba>();
            });
        });
        Cliente = Api.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, BaseAddress = new Uri("https://localhost") });
    }

    /// <summary>Correo único por prueba: todas comparten la misma base de datos.</summary>
    internal static string CorreoNuevo() => Guid.NewGuid() + "@example.com";

    internal Task<HttpResponseMessage> RegistroAsync(string correo, string contrasena = Contrasena, string? versionPolitica = "1.2") =>
        Cliente.PostAsJsonAsync("/api/v1/auth/registro", new
        {
            correo, contrasena, versionPoliticaAceptada = versionPolitica, versionTerminosAceptada = "1.2",
        }, Ct);

    /// <summary>Registra un conductor y devuelve su sesión (tokens de acceso y de renovación).</summary>
    internal async Task<JsonElement> RegistrarAsync(string correo)
    {
        var respuesta = await RegistroAsync(correo);
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return await respuesta.Content.ReadFromJsonAsync<JsonElement>(Ct);
    }

    internal Task<HttpResponseMessage> IniciarSesionAsync(string correo, string contrasena = Contrasena,
        string canal = "app", string? turnstileToken = null) =>
        Cliente.PostAsJsonAsync("/api/v1/auth/inicio-sesion", new { correo, contrasena, canal, turnstileToken }, Ct);

    internal Task<HttpResponseMessage> RenovarAsync(string? tokenRenovacion) =>
        Cliente.PostAsJsonAsync("/api/v1/auth/renovar", new { tokenRenovacion }, Ct);

    internal Task<HttpResponseMessage> VerificarCorreoAsync(string token) =>
        Cliente.PostAsJsonAsync("/api/v1/auth/verificar-correo", new { token }, Ct);

    internal Task<HttpResponseMessage> FijarContrasenaAsync(string token, string nuevaContrasena) =>
        Cliente.PostAsJsonAsync("/api/v1/auth/fijar-contrasena", new { token, nuevaContrasena }, Ct);

    internal Task<HttpResponseMessage> CerrarSesionAsync() => Cliente.PostAsync("/api/v1/auth/cerrar-sesion", null, Ct);

    internal Task<HttpResponseMessage> MisDatosAsync() => Cliente.GetAsync("/api/v1/me", Ct);

    internal async Task<HttpResponseMessage> EliminarMiCuentaAsync(string contrasena)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/me") { Content = JsonContent.Create(new { contrasena }) };
        return await Cliente.SendAsync(peticion, Ct);
    }

    internal void Autenticar(JsonElement sesion) => Cliente.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", sesion.GetProperty("tokenAcceso").GetString());

    internal void QuitarAutenticacion() => Cliente.DefaultRequestHeaders.Authorization = null;

    internal static string? TokenRenovacion(JsonElement sesion) => sesion.GetProperty("tokenRenovacion").GetString();

    /// <summary>El id interno viaja como «sub» del JWT; las pruebas no tienen otra forma de conocerlo.</summary>
    internal static Guid UsuarioId(JsonElement sesion) =>
        Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(sesion.GetProperty("tokenAcceso").GetString()).Subject);

    internal static async Task<string?> PropiedadAsync(HttpResponseMessage respuesta, string nombre) =>
        (await respuesta.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty(nombre).GetString();

    /// <summary>Código de error del contrato (ProblemDetails.codigo).</summary>
    internal static Task<string?> CodigoAsync(HttpResponseMessage respuesta) => PropiedadAsync(respuesta, "codigo");

    public void Dispose()
    {
        Cliente.Dispose();
        Api.Dispose();
    }
}
