using System.Net.Http.Json;
using KillaUru.Api.Observabilidad;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace KillaUru.Integracion.Tests.Seguridad;

/// <summary>
/// Spec 001 — FR-034 (T067, RN-09, RNF-32):
/// el enricher FiltroDatosPersonales está registrado en el pipeline HTTP real y elimina propiedades
/// sensibles antes de que lleguen al sink de salida. Complementa FiltroDatosPersonalesTests (unitario).
/// </summary>
public sealed class LogsSinDatosPersonalesTests(BaseDatosPrueba bd) : IClassFixture<BaseDatosPrueba>
{
    private static readonly string RutaLogContexto = "/api/v1/prueba/log-contexto";

    private static WebApplicationFactory<Program> ApiConSink(BaseDatosPrueba bd, SinkMemoria sink) =>
        new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.AddSerilog(c => c
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<FiltroDatosPersonales>()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .WriteTo.Sink(sink))));

    [Fact]
    public async Task Propiedades_prohibidas_se_eliminan_en_el_pipeline_http()
    {
        // Arrange — un seudónimo que se pusheará al LogContext en el controlador de prueba
        var sink = new SinkMemoria();
        using var api = ApiConSink(bd, sink);
        var cliente = api.CreateClient();
        var cuerpo = JsonContent.Create(new { propiedades = new Dictionary<string, string> { ["Seudonimo"] = "seudonimo-secreto-456" } });

        // Act
        await cliente.PostAsync(RutaLogContexto, cuerpo, TestContext.Current.CancellationToken);

        // Assert — el log fue emitido (sink no vacío) pero la propiedad Seudonimo fue eliminada (FR-034)
        Assert.NotEmpty(sink.Lineas);
        Assert.False(sink.ContienePropiedadConValor("Seudonimo", "seudonimo-secreto-456"),
            "El enricher debe eliminar la propiedad Seudonimo en el pipeline real");
    }

    [Fact]
    public async Task La_combinacion_sub_y_loteId_elimina_el_sub_en_el_pipeline()
    {
        // Arrange — el par Sub+LoteId viola la privacidad por correlación (RN-09)
        var sink = new SinkMemoria();
        var loteId = Guid.NewGuid().ToString();
        var subId = "cuenta-de-usuario-secreto";
        using var api = ApiConSink(bd, sink);
        var cliente = api.CreateClient();
        var cuerpo = JsonContent.Create(new
        {
            propiedades = new Dictionary<string, string> { ["Sub"] = subId, ["LoteId"] = loteId }
        });

        // Act
        await cliente.PostAsync(RutaLogContexto, cuerpo, TestContext.Current.CancellationToken);

        // Assert — Sub se elimina cuando coexiste con LoteId (FR-034); LoteId solo no revela identidad
        Assert.False(sink.ContienePropiedadConValor("Sub", subId),
            "Sub no debe aparecer en logs cuando LoteId también está presente");
        Assert.True(sink.ContienePropiedadConValor("LoteId", loteId),
            "LoteId sin Sub puede quedar para trazabilidad de lotes");
    }

    [Fact]
    public async Task El_bearer_token_de_una_peticion_real_no_aparece_en_los_logs()
    {
        // Arrange — un conductor hace una petición autenticada; el token no debe quedar en ningún log
        var sink = new SinkMemoria();
        var token = Tokens.Emitir(Roles.ConductorRegistrado);
        using var api = ApiConSink(bd, sink);
        var cliente = api.CreateClient();
        var peticion = Peticiones.Get("/api/v1/prueba/publica", token);

        // Act
        await cliente.SendAsync(peticion, TestContext.Current.CancellationToken);

        // Assert — el valor del token no debe aparecer en ningún log del request (RNF-32)
        Assert.NotEmpty(sink.Lineas);
        Assert.False(sink.ContieneTexto(token),
            "El valor del Bearer token no debe aparecer en ningún log del pipeline HTTP");
    }
}
