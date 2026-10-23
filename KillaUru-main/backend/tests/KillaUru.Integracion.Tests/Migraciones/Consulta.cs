using Npgsql;

namespace KillaUru.Integracion.Tests.Migraciones;

/// <summary>Atajos de lectura para verificar la estructura de la base de datos.</summary>
internal static class Consulta
{
    internal static async Task<List<string>> TextosAsync(string cadena, string sql)
    {
        await using var conexion = new NpgsqlConnection(cadena);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand(sql, conexion);
        await using var lector = await comando.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var filas = new List<string>();
        while (await lector.ReadAsync(TestContext.Current.CancellationToken))
        {
            filas.Add(lector.GetString(0));
        }

        return filas;
    }

    internal static async Task EjecutarAsync(string cadena, string sql)
    {
        await using var conexion = new NpgsqlConnection(cadena);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand(sql, conexion);
        await comando.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
