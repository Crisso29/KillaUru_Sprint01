using System.Net;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>T032: quién es cada petición y qué responde la API ante tokens buenos, malos y ausentes.</summary>
public sealed class SeguridadTests(BaseDatosPrueba bd) : IDisposable
{
    private readonly FabricaApi _api = new(bd);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Sin_token_el_rol_es_Anonimo()
    {
        var cuerpo = await _api.CreateClient().GetStringAsync("/api/v1/prueba/publica", Ct);

        Assert.Equal(Roles.Anonimo, cuerpo);
    }

    [Fact]
    public async Task Sin_token_en_un_endpoint_con_rol_es_401()
    {
        using var respuesta = await _api.CreateClient().GetAsync("/api/v1/prueba/conductor", Ct);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, "token_invalido");
    }

    [Fact]
    public async Task Con_un_rol_sin_permiso_es_403_sin_permiso()
    {
        var token = Tokens.Emitir(Roles.ConductorRegistrado);

        using var respuesta = await _api.CreateClient().SendAsync(Peticiones.Get("/api/v1/prueba/admin", token), Ct);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, "sin_permiso");
    }

    public static TheoryData<string> TokensInvalidos() => new()
    {
        Tokens.Emitir(Roles.ConductorRegistrado, venceEn: TimeSpan.FromSeconds(-1)),            // vencido hace 1 s (sin tolerancia)
        Tokens.Emitir(Roles.ConductorRegistrado, clave: "otra-clave-que-tambien-tiene-32-bytes-o-mas"),  // mal firmado
        Tokens.Emitir("Superusuario"),                                                          // rol desconocido
        "esto-no-es-un-jwt",
    };

    [Theory]
    [MemberData(nameof(TokensInvalidos))]
    public async Task Un_token_invalido_es_401_aunque_el_endpoint_sea_publico(string token)
    {
        using var respuesta = await _api.CreateClient().SendAsync(Peticiones.Get("/api/v1/prueba/publica", token), Ct);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, "token_invalido");
    }

    [Fact]
    public async Task El_token_en_la_cookie_ku_acceso_se_acepta()
    {
        var token = Tokens.Emitir(Roles.Administrador);

        using var respuesta = await _api.CreateClient().SendAsync(Peticiones.ConCookie("/api/v1/prueba/admin", token), Ct);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    public void Dispose() => _api.Dispose();

    internal static async Task AfirmarProblemaAsync(HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)
    {
        Assert.Equal(estado, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(Ct));
        Assert.Equal(codigo, json.RootElement.GetProperty("codigo").GetString());
        Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("correlacionId").GetString()));
    }
}
