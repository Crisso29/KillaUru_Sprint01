using System.Net;
using System.Net.Http.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// La autorización va antes que la validación del cuerpo en cada ruta administrativa, y el inicio
/// del panel exige anti-bot y muestra el bloqueo. Spec 003 · FR-011, FR-012, FR-042.
/// </summary>
public sealed class RespuestasComunesApiTests(BaseDatosPrueba bd)
{
    private static readonly Guid IdCualquiera = Guid.NewGuid();

    private static readonly (HttpMethod Metodo, string Ruta)[] RutasAdministrativas =
    [
        (HttpMethod.Get, "/api/v1/usuarios"), (HttpMethod.Post, "/api/v1/usuarios"),
        (HttpMethod.Put, $"/api/v1/usuarios/{IdCualquiera}/rol"), (HttpMethod.Post, $"/api/v1/usuarios/{IdCualquiera}/desactivar"),
        (HttpMethod.Post, $"/api/v1/usuarios/{IdCualquiera}/activar"), (HttpMethod.Post, $"/api/v1/usuarios/{IdCualquiera}/reenviar-correo"),
        (HttpMethod.Get, "/api/v1/usuarios/revision-privilegios"), (HttpMethod.Post, "/api/v1/usuarios/revision-privilegios"),
    ];

    private static async Task<HttpStatusCode[]> LlamarATodasAsync(EscenarioIdentidad escenario)
    {
        var estados = new List<HttpStatusCode>();
        foreach (var (metodo, ruta) in RutasAdministrativas)
        {
            using var peticion = new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(new { }) };
            estados.Add((await escenario.Cliente.SendAsync(peticion, EscenarioIdentidad.Ct)).StatusCode);
        }
        return [.. estados];
    }

    /// <summary>Sin token, toda ruta administrativa responde 401, aunque el cuerpo esté vacío (FR-042).</summary>
    [Fact]
    public async Task Sin_token_todas_las_rutas_administrativas_responden_401()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);

        // Act
        var estados = await LlamarATodasAsync(escenario);

        // Assert
        Assert.All(estados, estado => Assert.Equal(HttpStatusCode.Unauthorized, estado));
    }

    /// <summary>Con token de conductor, toda ruta administrativa responde 403 (FR-042).</summary>
    [Fact]
    public async Task Conductor_recibe_403_en_todas_las_rutas_administrativas()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        escenario.Autenticar(await escenario.RegistrarAsync(EscenarioIdentidad.CorreoNuevo()));

        // Act
        var estados = await LlamarATodasAsync(escenario);

        // Assert
        Assert.All(estados, estado => Assert.Equal(HttpStatusCode.Forbidden, estado));
    }

    /// <summary>El panel no acepta credenciales válidas sin el token anti-bot (FR-012).</summary>
    [Fact]
    public async Task Panel_sin_antibot_se_rechaza()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearAsync(escenario);

        // Act
        var respuesta = await escenario.IniciarSesionAsync(correo, canal: "panel");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    /// <summary>El panel informa el bloqueo con 423, Retry-After y la hora de fin (FR-011).</summary>
    [Fact]
    public async Task Panel_muestra_el_bloqueo_con_retry_after()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearAsync(escenario);
        for (var fallo = 0; fallo < 3; fallo++)
            await escenario.IniciarSesionAsync(correo, "incorrecta", "panel", EscenarioIdentidad.AntiBotValido);

        // Act
        var bloqueada = await escenario.IniciarSesionAsync(correo, canal: "panel", turnstileToken: EscenarioIdentidad.AntiBotValido);

        // Assert
        Assert.Equal(HttpStatusCode.Locked, bloqueada.StatusCode);
        Assert.NotNull(bloqueada.Headers.RetryAfter);
        Assert.Contains("bloqueadoHastaUtc", await bloqueada.Content.ReadAsStringAsync(EscenarioIdentidad.Ct));
    }
}
