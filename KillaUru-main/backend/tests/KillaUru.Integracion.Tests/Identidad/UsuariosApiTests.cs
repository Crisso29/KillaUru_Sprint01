using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Gestión de usuarios desde el panel y sesión del panel con cookies.
/// Spec 003 · FR-020, FR-030, FR-033, FR-034, FR-035.
/// </summary>
public sealed class UsuariosApiTests(BaseDatosPrueba bd)
{
    private static async Task<Guid> IdPorCorreoAsync(EscenarioIdentidad escenario, string correo)
    {
        var lista = await escenario.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/usuarios?busqueda=" + correo, EscenarioIdentidad.Ct);
        return lista.GetProperty("elementos")[0].GetProperty("id").GetGuid();
    }

    /// <summary>El administrador encuentra a un usuario por correo y por id (FR-030, FR-035).</summary>
    [Fact]
    public async Task Busqueda_por_correo_y_por_ids_encuentra_al_usuario()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearYAutenticarAsync(escenario);

        // Act
        var porCorreo = await escenario.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/usuarios?busqueda=" + correo, EscenarioIdentidad.Ct);
        var id = porCorreo.GetProperty("elementos")[0].GetProperty("id").GetGuid();
        var porIds = await escenario.Cliente.GetFromJsonAsync<JsonElement>($"/api/v1/usuarios?ids={id}", EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(1, porCorreo.GetProperty("total").GetInt32());
        Assert.Equal(1, porIds.GetProperty("total").GetInt32());
    }

    /// <summary>Un administrador no puede desactivarse a sí mismo: 409 (FR-033).</summary>
    [Fact]
    public async Task Administrador_no_puede_desactivarse_a_si_mismo()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearYAutenticarAsync(escenario);
        var propioId = await IdPorCorreoAsync(escenario, correo);

        // Act
        var respuesta = await escenario.Cliente.PostAsJsonAsync($"/api/v1/usuarios/{propioId}/desactivar", new { motivo = "prueba" }, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    /// <summary>Confirmar a todos los administradores activos guarda la revisión mensual: 201 (FR-034).</summary>
    [Fact]
    public async Task Revision_que_confirma_a_todos_se_guarda()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        await AdministradoresPrueba.CrearYAutenticarAsync(escenario);
        var estado = await escenario.Cliente.GetFromJsonAsync<JsonElement>("/api/v1/usuarios/revision-privilegios", EscenarioIdentidad.Ct);
        var confirmados = estado.GetProperty("administradores").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToArray();

        // Act
        var guardar = await escenario.Cliente.PostAsJsonAsync("/api/v1/usuarios/revision-privilegios",
            new { confirmados, retirados = Array.Empty<Guid>() }, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, guardar.StatusCode);
    }

    /// <summary>
    /// En el panel los tokens viajan solo en cookies HttpOnly, Secure y SameSite=Strict, nunca en
    /// el cuerpo; la de renovación solo se envía a su ruta (FR-020).
    /// </summary>
    [Fact]
    public async Task Panel_recibe_tokens_solo_en_cookies_seguras()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearAsync(escenario);

        // Act
        var login = await escenario.IniciarSesionAsync(correo, canal: "panel", turnstileToken: EscenarioIdentidad.AntiBotValido);

        // Assert
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False((await login.Content.ReadFromJsonAsync<JsonElement>(EscenarioIdentidad.Ct)).TryGetProperty("tokenAcceso", out _));
        var cookies = login.Headers.GetValues("Set-Cookie").ToArray();
        Assert.Equal(2, cookies.Length);
        Assert.All(cookies, c => { Assert.Contains("httponly", c); Assert.Contains("secure", c); Assert.Contains("samesite=strict", c); });
        Assert.Contains(cookies, c => c.StartsWith("ku_renovacion=", StringComparison.Ordinal)
            && c.Contains("path=/api/v1/auth/renovar", StringComparison.Ordinal));
    }

    /// <summary>Con el acceso ya vencido, la cookie de renovación basta para seguir en el panel (FR-020).</summary>
    [Fact]
    public async Task Panel_renueva_con_la_cookie_cuando_el_acceso_vencio()
    {
        // Arrange
        using var escenario = new EscenarioIdentidad(bd);
        var correo = await AdministradoresPrueba.CrearAsync(escenario);
        var login = await escenario.IniciarSesionAsync(correo, canal: "panel", turnstileToken: EscenarioIdentidad.AntiBotValido);
        var cookies = login.Headers.GetValues("Set-Cookie").Select(c => c.Split(';')[0]);
        escenario.Cliente.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies));
        escenario.Reloj.Advance(TimeSpan.FromMinutes(6));

        // Act
        var renovar = await escenario.Cliente.PostAsync("/api/v1/auth/renovar", null, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.OK, renovar.StatusCode);
        Assert.False((await renovar.Content.ReadFromJsonAsync<JsonElement>(EscenarioIdentidad.Ct)).TryGetProperty("tokenAcceso", out _));
    }
}
