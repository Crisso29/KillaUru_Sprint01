using System.Text.RegularExpressions;
using DotNet.Testcontainers.Containers;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>
/// Reproduce lo que hace deploy/migrar.sh, pero dentro del contenedor de pruebas: el mismo psql,
/// con ON_ERROR_STOP, una transacción por script y su registro en migraciones.aplicada.
/// </summary>
/// <param name="contenedor">Contenedor de PostgreSQL ya iniciado.</param>
internal sealed class MigradorPrueba(IContainer contenedor)
{
    internal const string CarpetaEnContenedor = "/migraciones/";

    private static readonly Regex Fase = new(@"^-- .* · fase ([12])", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>V0000 como superusuario, con las contraseñas como variables de psql.</summary>
    internal Task<ExecResult> AplicarArranqueAsync() => EjecutarAsync(
        "postgres", BaseDatosPrueba.ClaveSuperusuario,
        $"-v pw_migrador={BaseDatosPrueba.ClaveMigrador} -v pw_app={BaseDatosPrueba.ClaveApp} -f {CarpetaEnContenedor}V0000__arranque.sql",
        exigirExito: true);

    /// <summary>Una migración versionada como killauru_migrador, registrada en la misma transacción.</summary>
    /// <param name="archivoLocal">Ruta local del script; debe estar en db/migraciones.</param>
    /// <param name="exigirExito">Si es true, una falla de psql rompe la prueba.</param>
    /// <returns>Resultado de psql.</returns>
    internal Task<ExecResult> AplicarAsync(string archivoLocal, bool exigirExito = true)
    {
        var nombre = Path.GetFileNameWithoutExtension(archivoLocal);
        var version = nombre.Split("__")[0];
        var descripcion = nombre.Split("__")[1];
        var coincidencia = Fase.Match(File.ReadAllText(archivoLocal));
        var fase = coincidencia.Success ? coincidencia.Groups[1].Value : "1";
        var registro = $"INSERT INTO migraciones.aplicada VALUES ('{version}', '{descripcion}', clock_timestamp(), {fase})";
        return EjecutarAsync(
            "killauru_migrador", BaseDatosPrueba.ClaveMigrador,
            $"-f {CarpetaEnContenedor}{Path.GetFileName(archivoLocal)} -c \"{registro}\"", exigirExito);
    }

    /// <summary>Copia un script al contenedor (para probar uno que falla a propósito).</summary>
    /// <param name="archivoLocal">Ruta local del script.</param>
    /// <returns>Tarea de la copia.</returns>
    internal Task CopiarAsync(string archivoLocal) =>
        contenedor.CopyAsync(new FileInfo(archivoLocal), CarpetaEnContenedor);

    private async Task<ExecResult> EjecutarAsync(string usuario, string clave, string argumentos, bool exigirExito)
    {
        var comando = $"PGPASSWORD={clave} psql -h 127.0.0.1 -d killauru -U {usuario} -v ON_ERROR_STOP=1 -X -q --single-transaction {argumentos}";
        var resultado = await contenedor.ExecAsync(["sh", "-c", comando]);
        if (exigirExito && resultado.ExitCode != 0)
        {
            throw new InvalidOperationException($"psql falló ({resultado.ExitCode}): {resultado.Stderr}");
        }

        return resultado;
    }
}
