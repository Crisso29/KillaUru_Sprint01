using System.Net;
using System.Net.Http.Json;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Auditoria.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using EstadoAnomalia = KillaUru.Anomalias.Domain.EstadoAnomalia;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// ADR-013: si Auditoría falla, la acción del administrador no ocurre. La moderación se revierte y la
/// exportación se corta antes de entregar el archivo. Spec 004 · FR-025, FR-028.
/// </summary>
[Collection("Anomalias")]
public sealed class AtomicidadAuditoriaTests(BaseDatosPrueba bd)
{
    /// <summary>Auditoría que falla tanto al escribir como al registrar una lectura.</summary>
    private static IAuditoriaService AuditoriaQueFalla()
    {
        var auditoria = Substitute.For<IAuditoriaService>();
        var fallo = Task.FromException(new InvalidOperationException("fallo simulado"));
        auditoria.RegistrarAsync(Arg.Any<AccionAuditada>(), Arg.Any<ActorAuditoria>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>()).Returns(fallo);
        auditoria.RegistrarLecturaAsync(Arg.Any<AccionAuditada>(), Arg.Any<ActorAuditoria>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>()).Returns(fallo);
        return auditoria;
    }

    private static HttpClient AdministradorSinAuditoria(EscenarioAnomalias escenario, out WebApplicationFactory<Program> api)
    {
        var auditoria = AuditoriaQueFalla();
        api = escenario.Api.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton(auditoria)));
        var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", Tokens.Emitir("Administrador", reloj: escenario.Reloj));
        return cliente;
    }

    /// <summary>Descartar con Auditoría caída responde 500 y la anomalía sigue Activa, sin transición nueva (FR-025).</summary>
    [Fact]
    public async Task Fallo_de_auditoria_revierte_estado_e_historial()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        using var cliente = AdministradorSinAuditoria(escenario, out var api);
        using var _ = api;

        // Act
        using var respuesta = await cliente.PostAsJsonAsync($"/api/v1/anomalies/{id}/moderacion", new { accion = "Descartar" }, EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
        Assert.Equal(EstadoAnomalia.Activa, (await escenario.LeerAnomaliaAsync(id)).Estado);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var transiciones = await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Transiciones.CountAsync(t => t.AnomaliaId == id, EscenarioAnomalias.Ct);
        Assert.Equal(1, transiciones);
    }

    /// <summary>Exportar con Auditoría caída devuelve un problema JSON y ninguna fila del dataset (FR-028).</summary>
    [Fact]
    public async Task Fallo_de_auditoria_no_entrega_archivo()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        using var cliente = AdministradorSinAuditoria(escenario, out var api);
        using var _ = api;

        // Act
        using var respuesta = await cliente.PostAsJsonAsync("/api/v1/exportaciones", new { formato = "CSV" }, EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType!.MediaType);
        Assert.DoesNotContain("primer_reporte", await respuesta.Content.ReadAsStringAsync(EscenarioAnomalias.Ct), StringComparison.Ordinal);
    }
}
