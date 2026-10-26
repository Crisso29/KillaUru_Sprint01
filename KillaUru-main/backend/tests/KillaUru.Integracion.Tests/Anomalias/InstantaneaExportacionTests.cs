using KillaUru.Anomalias.Application.Exportacion;
using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// La exportación lee de una instantánea: lo que se audita (el conteo) es exactamente lo que se
/// entrega, aunque otra conexión cambie datos en medio. Spec 004 · FR-028.
/// </summary>
[Collection("Anomalias")]
public sealed class InstantaneaExportacionTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Conteo_y_lectura_conservan_la_instantanea_ante_cambios_concurrentes()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        await using var lector = escenario.Api.Services.CreateAsyncScope();
        var exportacion = lector.ServiceProvider.GetRequiredService<IExportacionRepository>();
        var soloActivas = new SolicitudExportacion("CSV", Estados: ["Activa"]);
        await using var instantanea = await exportacion.AbrirInstantaneaAsync(EscenarioAnomalias.Ct);
        var conteoAuditado = await exportacion.ContarAsync(soloActivas, EscenarioAnomalias.Ct);

        // Act — otra conexión la descarta entre el conteo y la lectura
        await escenario.EjecutarSqlAsync($"UPDATE anomalias.anomalia SET estado=5 WHERE id={id}");
        var filas = new List<FilaExportacion>();
        await foreach (var fila in exportacion.LeerAsync(soloActivas, EscenarioAnomalias.Ct)) filas.Add(fila);

        // Assert
        Assert.Equal(conteoAuditado, filas.Count);
        Assert.Contains(filas, f => f.Id == id && f.Estado == "Activa");
    }
}
