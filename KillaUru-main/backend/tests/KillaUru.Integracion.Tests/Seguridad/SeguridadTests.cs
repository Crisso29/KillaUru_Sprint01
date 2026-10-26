using System.Net;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// Spec 001 — FR-008, FR-009, FR-012, FR-045 (T032, T-SEG09, T-SEG20):
/// el middleware de sesión resuelve el rol, rechaza tokens inválidos y aplica
/// la regla 401 / 403 antes de que el endpoint ejecute cualquier lógica.
/// </summary>
public sealed class SeguridadTests(BaseDatosPrueba bd) : IDisposable
{
    private readonly FabricaApi _api = new(bd);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Sin_token_el_rol_resuelto_es_Anonimo()
    {
        // Arrange — petición sin cabecera Authorization ni cookie ku_acceso
        var cliente = _api.CreateClient();

        // Act
        var cuerpo = await cliente.GetStringAsync("/api/v1/prueba/publica", Ct);

        // Assert — el middleware asigna Anonimo cuando no hay credencial (FR-008)
        Assert.Equal(Roles.Anonimo, cuerpo);
    }

    [Fact]
    public async Task Sin_token_en_un_endpoint_con_rol_es_401_con_codigo_estable()
    {
        // Arrange — endpoint que exige ConductorRegistrado o Administrador
        var cliente = _api.CreateClient();

        // Act
        using var respuesta = await cliente.GetAsync("/api/v1/prueba/conductor", Ct);

        // Assert — 401 con código estable y correlacionId (FR-008, FR-012, T-SEG20)
        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, "token_invalido");
    }

    [Fact]
    public async Task Con_token_valido_pero_rol_insuficiente_es_403_sin_permiso()
    {
        // Arrange — conductor intenta acceder a un endpoint reservado al administrador
        var token = Tokens.Emitir(Roles.ConductorRegistrado);

        // Act
        using var respuesta = await _api.CreateClient().SendAsync(
            Peticiones.Get("/api/v1/prueba/admin", token), Ct);

        // Assert — 403 distinto de 401: el usuario existe pero no tiene el rol (FR-045, T-SEG20)
        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, "sin_permiso");
    }

    /// <summary>
    /// Casos que deben devolver 401 en cualquier endpoint, incluidos los públicos:
    /// vencido hace 1 s (sin tolerancia de reloj, T-SEG09), firma incorrecta,
    /// rol desconocido y cadena que no es un JWT.
    /// </summary>
    public static TheoryData<string> TokensInvalidos() => new()
    {
        // Vencido hace exactamente 1 segundo — ClockSkew = TimeSpan.Zero (FR-008, RNF-26, T-SEG09)
        Tokens.Emitir(Roles.ConductorRegistrado, venceEn: TimeSpan.FromSeconds(-1)),
        // Firma incorrecta — la clave es diferente a la configurada en el host
        Tokens.Emitir(Roles.ConductorRegistrado, clave: "otra-clave-que-tambien-tiene-32-bytes-o-mas"),
        // Rol desconocido — no se degrada a Anónimo, se rechaza (FR-008)
        Tokens.Emitir("Superusuario"),
        // Cadena arbitraria — no es un JWT válido
        "esto-no-es-un-jwt",
    };

    [Theory]
    [MemberData(nameof(TokensInvalidos))]
    public async Task Un_token_invalido_siempre_es_401_aunque_el_endpoint_acepte_anonimos(string token)
    {
        // Arrange — endpoint público (acepta Anonimo), pero se envía un token con algún defecto
        var peticion = Peticiones.Get("/api/v1/prueba/publica", token);

        // Act
        using var respuesta = await _api.CreateClient().SendAsync(peticion, Ct);

        // Assert — el middleware rechaza el token defectuoso antes de resolver el rol (FR-008)
        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, "token_invalido");
    }

    [Fact]
    public async Task El_token_en_cookie_ku_acceso_HttpOnly_autentica_al_panel()
    {
        // Arrange — el panel envía el token en la cookie, no en la cabecera (ADR-009, FR-009)
        var token = Tokens.Emitir(Roles.Administrador);

        // Act
        using var respuesta = await _api.CreateClient().SendAsync(
            Peticiones.ConCookie("/api/v1/prueba/admin", token), Ct);

        // Assert — el middleware acepta la cookie ku_acceso como fuente de token (FR-009)
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    public void Dispose() => _api.Dispose();

    /// <summary>
    /// Helper compartido: verifica que toda respuesta de error sea RFC 9457 ProblemDetails
    /// con código estable y correlacionId sin datos internos (FR-012, constitución IX).
    /// </summary>
    internal static async Task AfirmarProblemaAsync(
        HttpResponseMessage respuesta, HttpStatusCode estado, string codigo)
    {
        // Estado HTTP esperado
        Assert.Equal(estado, respuesta.StatusCode);
        // Tipo de contenido obligatorio para errores (RFC 9457)
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(Ct));
        // Código estable para que el cliente distinga el tipo de error sin parsear el título
        Assert.Equal(codigo, json.RootElement.GetProperty("codigo").GetString());
        // correlacionId permite cruzar el error con los logs de Grafana (FR-012)
        Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("correlacionId").GetString()));
    }
}
