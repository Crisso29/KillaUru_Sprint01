using Npgsql;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>Datos sintéticos aislados por actor, con timestamps iguales para probar el desempate por id.</summary>
internal static class SemillasAuditoria
{
    internal static async Task InsertarAsync(string cadena, Guid actor, int cantidad)
    {
        await using var conexion = new NpgsqlConnection(cadena);
        await conexion.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand("""
            INSERT INTO auditoria.registro (ocurrido_utc, actor_id, actor_rol, accion, objetivo, contexto)
            SELECT @fecha, @actor, 2, 1, 'prueba-sintetica', '{}'::jsonb
            FROM generate_series(1, @cantidad)
            """, conexion);
        comando.Parameters.AddWithValue("fecha", new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));
        comando.Parameters.AddWithValue("actor", actor);
        comando.Parameters.AddWithValue("cantidad", cantidad);
        await comando.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
