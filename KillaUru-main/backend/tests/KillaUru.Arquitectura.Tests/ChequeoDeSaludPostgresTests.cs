using System.Text.RegularExpressions;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// DT-03 / 001-T083: el chequeo de salud de PostgreSQL debe preguntar por TCP (<c>pg_isready -h 127.0.0.1</c>).
/// En el primer arranque la imagen oficial levanta un servidor temporal solo por socket Unix, inicializa la base y se
/// reinicia; <c>pg_isready</c> sin <c>-h</c> lo da por sano en ese momento y el migrador se conecta justo durante el
/// reinicio (<c>exit 2</c>, killauru-local, 2026-11-22). Por TCP solo responde el servidor definitivo.
/// Se revisan todos los <c>docker-compose*.yml</c> de <c>deploy/</c>, también los que se agreguen después.
/// </summary>
public sealed class ChequeoDeSaludPostgresTests
{
    private const int ComposeConBaseDeDatos = 3;   // producción, pruebas y local

    private static readonly Regex Chequeo = new(@"pg_isready[^""\]\r\n]*", RegexOptions.Compiled);

    private static IEnumerable<(string Archivo, string Comando)> Chequeos() =>
        Directory.GetFiles(Path.Combine(Repositorio.Raiz, "deploy"), "docker-compose*.yml")
            .SelectMany(ruta => Chequeo.Matches(File.ReadAllText(ruta))
                .Select(coincidencia => (Path.GetFileName(ruta), coincidencia.Value)));

    [Fact]
    public void Todo_compose_con_base_de_datos_la_chequea_por_tcp()
    {
        // Arrange
        var chequeos = Chequeos().ToList();

        // Act
        var porSocket = chequeos.Where(chequeo => !chequeo.Comando.Contains("-h 127.0.0.1", StringComparison.Ordinal))
            .Select(chequeo => $"{chequeo.Archivo}: {chequeo.Comando}")
            .ToList();

        // Assert — sin la primera aserción, una búsqueda vacía «pasaría» sin revisar nada
        Assert.Equal(ComposeConBaseDeDatos, chequeos.Select(chequeo => chequeo.Archivo).Distinct().Count());
        Assert.True(porSocket.Count == 0, "Chequeos por socket (carrera del primer arranque):\n" + string.Join("\n", porSocket));
    }
}
