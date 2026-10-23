using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace KillaUru.Api.Salud;

/// <summary>¿Responde PostgreSQL? (RF-28). Un <c>SELECT 1</c> como killauru_app.</summary>
/// <param name="baseDatos">Fuente de conexiones de la API.</param>
/// <param name="reloj">Reloj del sistema.</param>
internal sealed class ChequeoBaseDatos(NpgsqlDataSource baseDatos, TimeProvider reloj) : ChequeoConCache(reloj)
{
    protected override async Task<HealthCheckResult> ConsultarAsync(CancellationToken ct)
    {
        await using var comando = baseDatos.CreateCommand("SELECT 1");
        await comando.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return HealthCheckResult.Healthy();
    }
}
