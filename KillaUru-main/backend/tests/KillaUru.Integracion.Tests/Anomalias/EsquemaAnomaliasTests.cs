using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Las restricciones y la privacidad se comprueban en el esquema migrado, no solo en el modelo de EF:
/// la base rechaza por sí misma datos imposibles. Spec 004 · FR-015, FR-023.
/// </summary>
[Collection("Anomalias")]
public sealed class EsquemaAnomaliasTests(BaseDatosPrueba bd)
{
    private static FormattableString SentenciaInvalida(string caso, Guid anomaliaId, Guid eventoId) => caso switch
    {
        "persistencia fuera del catálogo" => $"UPDATE anomalias.anomalia SET persistencia=2 WHERE id={anomaliaId}",
        "confidence mayor que 1" => $"UPDATE anomalias.reporte_anomalia SET confidence=1.01 WHERE evento_id={eventoId}",
        "precisión negativa" => $"UPDATE anomalias.reporte_anomalia SET precision_m=-1 WHERE evento_id={eventoId}",
        "transición sin cambio de estado" => $"UPDATE anomalias.transicion SET desde=hacia WHERE anomalia_id={anomaliaId}",
        _ => $"UPDATE anomalias.transicion SET hacia=6 WHERE anomalia_id={anomaliaId}",
    };

    private static Task<List<string>> ColumnasAsync(AnomaliasDbContext db) => db.Database.SqlQuery<string>(
        $"SELECT column_name AS \"Value\" FROM information_schema.columns WHERE table_schema='anomalias'").ToListAsync(EscenarioAnomalias.Ct);

    /// <summary>Esquemas a los que apuntan las claves foráneas de Anomalías.</summary>
    private static Task<List<string>> EsquemasReferenciadosAsync(AnomaliasDbContext db) => db.Database.SqlQuery<string>($"""
        SELECT ccu.table_schema AS "Value" FROM information_schema.constraint_column_usage ccu
        JOIN information_schema.table_constraints tc ON tc.constraint_name=ccu.constraint_name AND tc.constraint_schema=ccu.constraint_schema
        WHERE tc.table_schema='anomalias' AND tc.constraint_type='FOREIGN KEY'
        """).ToListAsync(EscenarioAnomalias.Ct);

    /// <summary>Cada caso intenta guardar un valor que el CHECK de la migración debe rechazar.</summary>
    [Theory]
    [InlineData("persistencia fuera del catálogo")]
    [InlineData("confidence mayor que 1")]
    [InlineData("precisión negativa")]
    [InlineData("transición sin cambio de estado")]
    [InlineData("estado destino inexistente")]
    public async Task Check_rechaza_datos_invalidos(string caso)
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var evento = escenario.Evento();
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(evento);

        // Act
        var error = await Assert.ThrowsAsync<PostgresException>(() => escenario.EjecutarSqlAsync(SentenciaInvalida(caso, id, evento.EventoId)));

        // Assert
        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    /// <summary>
    /// Ninguna columna guarda cuentas, correos ni tokens, y no hay claves foráneas hacia otros esquemas:
    /// Anomalías no puede ligar una detección a una persona (FR-023).
    /// </summary>
    [Fact]
    public async Task No_hay_columnas_de_identidad_ni_enlaces_a_cuentas()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        string[] prohibidas = ["correo", "cuenta", "token"];

        // Act
        var columnas = await ColumnasAsync(db);
        var esquemasReferenciados = await EsquemasReferenciadosAsync(db);

        // Assert
        Assert.DoesNotContain(columnas, c => prohibidas.Any(p => c.Contains(p, StringComparison.OrdinalIgnoreCase)));
        Assert.All(esquemasReferenciados, esquema => Assert.Equal("anomalias", esquema));
    }
}
