using Npgsql;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Lee `ingesta.evento_recibido` del PostgreSQL de pruebas para comprobar efectos, no solo la respuesta HTTP.
/// Filtra siempre por los eventoId de la prueba: la base es compartida por toda la ejecución.
/// </summary>
internal static class RecepcionEnBd
{
    private const string Sql =
        "SELECT evento_id, lote_id, estado, con_sesion FROM ingesta.evento_recibido WHERE evento_id = ANY(@ids)";

    private static readonly TimeSpan Sondeo = TimeSpan.FromMilliseconds(200);

    internal static async Task<IReadOnlyList<FilaRecibida>> LeerAsync(string cadena, IEnumerable<Guid> ids)
    {
        await using var conexion = new NpgsqlConnection(cadena);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand(Sql, conexion);
        comando.Parameters.AddWithValue("ids", ids.ToArray());
        await using var lector = await comando.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var filas = new List<FilaRecibida>();
        while (await lector.ReadAsync(TestContext.Current.CancellationToken))
        {
            filas.Add(new(lector.GetGuid(0), lector.IsDBNull(1) ? null : lector.GetGuid(1), lector.GetInt16(2), lector.GetBoolean(3)));
        }

        return filas;
    }

    /// <summary>
    /// Espera hasta que ninguna fila siga en Recibido (0) o venza <paramref name="plazo"/>.
    /// Es un sondeo con plazo sobre una condición observable, no una espera fija.
    /// </summary>
    internal static async Task<IReadOnlyList<FilaRecibida>> EsperarProcesadasAsync(string cadena, IReadOnlyCollection<Guid> ids, TimeSpan plazo)
    {
        var limite = TimeProvider.System.GetUtcNow() + plazo;
        var filas = await LeerAsync(cadena, ids);
        while (filas.Any(f => f.Estado == 0) && TimeProvider.System.GetUtcNow() < limite)
        {
            await Task.Delay(Sondeo, TestContext.Current.CancellationToken);
            filas = await LeerAsync(cadena, ids);
        }

        return filas;
    }
}
