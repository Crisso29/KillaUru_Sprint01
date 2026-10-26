using KillaUru.Api.Observabilidad;
using Serilog.Events;
using Serilog.Parsing;

namespace KillaUru.Integracion.Tests.Observabilidad;

/// <summary>
/// Spec 001 — FR-034 (T067, RN-09, RNF-32):
/// el enricher de Serilog elimina propiedades sensibles de los logs antes de que lleguen a Grafana.
/// Cubre propiedades de primer nivel; las trazas de OpenTelemetry tienen su propia validación.
/// </summary>
public sealed class FiltroDatosPersonalesTests
{
    /// <summary>
    /// Construye un evento Serilog sintético con las propiedades indicadas.
    /// Permite probar el filtro de forma aislada, sin levantar la API completa.
    /// </summary>
    private static LogEvent Evento(params (string Nombre, object Valor)[] propiedades) => new(
        DateTimeOffset.UnixEpoch, LogEventLevel.Information, null,
        new MessageTemplateParser().Parse("x"),
        propiedades.Select(p => new LogEventProperty(p.Nombre, new ScalarValue(p.Valor))));

    /// <summary>
    /// Propiedades que nunca deben aparecer en los logs (FR-034, RN-09):
    /// seudónimo, IP, tokens, cookies, correos.
    /// </summary>
    [Theory]
    [InlineData("Seudonimo")]        // identifica al dispositivo (RN-09)
    [InlineData("RemoteIpAddress")]  // IP real del cliente
    [InlineData("Authorization")]    // cabecera con el token de acceso
    [InlineData("Cookie")]           // cookie ku_acceso del panel
    [InlineData("AccessToken")]      // token en cuerpo o atributo
    [InlineData("Correo")]           // datos personales directos
    [InlineData("Email")]            // alias en inglés del correo
    public void El_enricher_elimina_la_propiedad_sensible_y_conserva_las_no_sensibles(string nombre)
    {
        // Arrange — evento con la propiedad sensible y una no sensible juntas
        var evento = Evento((nombre, "dato-sensible"), ("Ruta", "/api/v1/prueba"));

        // Act
        new FiltroDatosPersonales().Enrich(evento, null!);

        // Assert — la propiedad sensible desaparece; la no sensible permanece (FR-034)
        Assert.False(evento.Properties.ContainsKey(nombre),
            $"La propiedad '{nombre}' no debe aparecer en los logs");
        Assert.True(evento.Properties.ContainsKey("Ruta"),
            "Las propiedades no sensibles deben conservarse");
    }

    [Fact]
    public void Sub_y_LoteId_juntos_violan_la_privacidad_por_correlacion_y_se_elimina_Sub()
    {
        // Arrange — el par sub+loteId permite vincular una cuenta con eventos de ubicación (RN-09)
        var evento = Evento(("Sub", "cuenta-1"), ("LoteId", Guid.NewGuid()));

        // Act
        new FiltroDatosPersonales().Enrich(evento, null!);

        // Assert — Sub se elimina cuando LoteId está presente (FR-034: el par es prohibido)
        Assert.False(evento.Properties.ContainsKey("Sub"),
            "Sub no debe coexistir con LoteId en un mismo evento de log");
        Assert.True(evento.Properties.ContainsKey("LoteId"),
            "LoteId solo, sin Sub, no revela la identidad");
    }

    [Fact]
    public void Sub_sin_LoteId_se_conserva_para_poder_auditar_accesos_administrativos()
    {
        // Arrange — Sub solo identifica quién accedió, sin revelar qué eventos envió
        var evento = Evento(("Sub", "cuenta-1"));

        // Act
        new FiltroDatosPersonales().Enrich(evento, null!);

        // Assert — Sub sin LoteId es útil para auditar accesos (FR-034: solo se prohíbe el par)
        Assert.True(evento.Properties.ContainsKey("Sub"),
            "Sub sin LoteId debe conservarse para trazabilidad de accesos");
    }
}
