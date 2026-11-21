using KillaUru.Integracion.Tests.Infraestructura;
using Npgsql;

namespace KillaUru.Integracion.Tests.Dashboard;

/// <summary>T022 / RN-17: el esquema dashboard solo contiene columnas sin PII.</summary>
public sealed class EsquemaPrivacidadTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Esquema_dashboard_solo_tiene_columnas_sin_pii()
    {
        // Arrange
        var columnasEsperadas = new HashSet<string> { "dia", "indicador", "valor" };

        // Act
        await using var conn = new NpgsqlConnection(bd.CadenaSuperusuario);
        await conn.OpenAsync(EscenarioDashboard.Ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema = 'dashboard'";
        await using var reader = await cmd.ExecuteReaderAsync(EscenarioDashboard.Ct);
        var columnas = new HashSet<string>();
        while (await reader.ReadAsync(EscenarioDashboard.Ct))
            columnas.Add(reader.GetString(0));

        // Assert
        Assert.Equal(columnasEsperadas, columnas);
    }
}
