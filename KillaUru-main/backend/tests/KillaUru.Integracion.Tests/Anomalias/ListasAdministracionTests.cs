using System.Text.Json;
using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Las listas del panel entregan lo necesario para decidir (severidad, reportes, vehículos) sin
/// divulgar dispositivos. Spec 004 · FR-027.
/// </summary>
[Collection("Anomalias")]
public sealed class ListasAdministracionTests(BaseDatosPrueba bd)
{
    private static async Task<JsonElement> ElementoAsync(HttpClient cliente, string ruta, Guid id)
    {
        using var lista = JsonDocument.Parse(await cliente.GetStringAsync(ruta, EscenarioAnomalias.Ct));
        return lista.RootElement.EnumerateArray().Single(a => a.GetProperty("id").GetGuid() == id).Clone();
    }

    private static async Task<JsonElement> ObtenerAsync(HttpClient cliente, string ruta)
    {
        using var documento = JsonDocument.Parse(await cliente.GetStringAsync(ruta, EscenarioAnomalias.Ct));
        return documento.RootElement.Clone();
    }

    /// <summary>Una grave recién nacida aparece en «Esperan confirmación» con su severidad y su único reporte.</summary>
    [Fact]
    public async Task Pendiente_muestra_severidad_y_reportes()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento(5));
        using var cliente = escenario.ClienteComo("Administrador");

        // Act
        var pendiente = await ElementoAsync(cliente, "/api/v1/moderacion/pendientes", id);

        // Assert
        Assert.Equal(5, pendiente.GetProperty("severidadMostrada").GetInt32());
        Assert.Equal(1, pendiente.GetProperty("reportes").GetInt32());
    }

    /// <summary>
    /// Una Descartada con 3 vehículos aparece en «Por revisar»; su detalle no tiene vencimiento y
    /// cuenta los vehículos de la revisión.
    /// </summary>
    [Fact]
    public async Task Por_revisar_y_detalle_cuentan_los_vehiculos()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento(5));
        var ahora = escenario.Reloj.GetUtcNow();
        await escenario.EjecutarSqlAsync($"UPDATE anomalias.anomalia SET estado=5, por_revisar=true, marcas_revision=ARRAY[{ahora},{ahora},{ahora}] WHERE id={id}");
        using var cliente = escenario.ClienteComo("Administrador");

        // Act
        var revision = await ElementoAsync(cliente, "/api/v1/moderacion/por-revisar", id);
        var detalle = await ObtenerAsync(cliente, $"/api/v1/anomalies/{id}");

        // Assert
        Assert.Equal(5, revision.GetProperty("severidadMostrada").GetInt32());
        Assert.Equal(3, revision.GetProperty("vehiculos7d").GetInt32());
        Assert.Equal(JsonValueKind.Null, detalle.GetProperty("venceUtc").ValueKind);
        Assert.Equal(3, detalle.GetProperty("vehiculosRevision").GetInt32());
    }

    /// <summary>Las transiciones recientes respetan el límite pedido e informan el actor (nulo si fue automática).</summary>
    [Fact]
    public async Task Transiciones_recientes_respetan_el_limite_e_informan_el_actor()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        await escenario.ProcesarAsync(escenario.Evento());
        using var cliente = escenario.ClienteComo("Administrador");

        // Act
        var transiciones = await ObtenerAsync(cliente, "/api/v1/anomalies/transiciones?limite=1");

        // Assert
        Assert.Equal(1, transiciones.GetArrayLength());
        Assert.True(transiciones[0].TryGetProperty("actorId", out _));
    }
}
