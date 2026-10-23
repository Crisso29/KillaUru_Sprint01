using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace KillaUru.Api.Salud;

/// <summary>
/// ¿La base tiene todas las migraciones que esta versión de la API espera? (T-OPS19). Las versiones
/// esperadas viajan dentro del ensamblado (los V*.sql como recursos), así no hay un número que
/// alguien olvide actualizar. Si falta una, /health/ready da 503 y el despliegue se detiene.
/// </summary>
/// <param name="baseDatos">Fuente de conexiones de la API.</param>
/// <param name="reloj">Reloj del sistema.</param>
internal sealed class ChequeoMigraciones(NpgsqlDataSource baseDatos, TimeProvider reloj) : ChequeoConCache(reloj)
{
    private const string PrefijoRecurso = "migraciones.";

    private static readonly string[] Esperadas = typeof(ChequeoMigraciones).Assembly.GetManifestResourceNames()
        .Where(n => n.StartsWith(PrefijoRecurso + "V", StringComparison.Ordinal))
        .Select(n => n[PrefijoRecurso.Length..].Split("__")[0])
        .ToArray();

    protected override async Task<HealthCheckResult> ConsultarAsync(CancellationToken ct)
    {
        await using var comando = baseDatos.CreateCommand("SELECT version FROM migraciones.aplicada");
        await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);
        var aplicadas = new HashSet<string>(StringComparer.Ordinal);
        while (await lector.ReadAsync(ct).ConfigureAwait(false))
        {
            aplicadas.Add(lector.GetString(0));
        }

        return Esperadas.All(aplicadas.Contains)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Hay migraciones pendientes");
    }
}
