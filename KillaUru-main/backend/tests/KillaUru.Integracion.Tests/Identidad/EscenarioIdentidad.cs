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

/// <summary>Host real, PostgreSQL real y salidas externas sustituidas para pruebas reproducibles.</summary>
internal sealed class EscenarioIdentidad : IDisposable
{
    internal CorreoDePrueba Correo { get; } = new();
    internal FakeTimeProvider Reloj { get; } = new(TimeProvider.System.GetUtcNow());
    internal WebApplicationFactory<Program> Api { get; }
    internal HttpClient Cliente { get; }
    internal const string Contrasena = "una-clave-larga";

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

    internal async Task<JsonElement> RegistrarAsync(string correo)
    {
        var respuesta = await Cliente.PostAsJsonAsync("/api/v1/auth/registro", new
        {
            correo, contrasena = Contrasena, versionPoliticaAceptada = "1.2", versionTerminosAceptada = "1.2",
        }, TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.Created, respuesta.StatusCode);
        return await respuesta.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }

    internal void Autenticar(JsonElement sesion) => Cliente.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", sesion.GetProperty("tokenAcceso").GetString());
    public void Dispose() { Cliente.Dispose(); Api.Dispose(); }
}
