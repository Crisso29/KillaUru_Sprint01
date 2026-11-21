using Npgsql;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>Consultas parametrizadas de comprobación; no comparten el contexto EF que se está probando.</summary>
internal static class DatosAuditoria
{
    internal static async Task<long> ContarAsync(string cadena, string objetivo)
    {
        await using var conexion = new NpgsqlConnection(cadena);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand("SELECT count(*) FROM auditoria.registro WHERE objetivo = @objetivo", conexion);
        comando.Parameters.AddWithValue("objetivo", objetivo);
        return (long)(await comando.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
    }
}
