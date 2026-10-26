using System.Globalization;
using System.Text.Json;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// Con la polilínea real de la PE-28A: el kilometraje coincide con las referencias compartidas con
/// la app Android y la longitud sale del eje completo, no de un valor inventado. Spec 004 · FR-003, FR-032.
/// </summary>
public sealed class CorredorRealTests
{
    private const double MargenMetros = 50;

    /// <summary>Los 20 puntos de referencia caen sobre la vía y dan el mismo km que usa Android, a menos de 1 m.</summary>
    [Fact]
    public void Veinte_referencias_compartidas_coinciden_a_menos_de_un_metro()
    {
        // Arrange
        using var datos = LeerGeoJson();
        var corredor = Crear(datos);
        var referencias = File.ReadAllLines(Ruta("km_referencia.csv")).Skip(1).Select(LeerReferencia).ToArray();

        // Act + Assert — cada referencia se comprueba por separado para saber cuál falla
        Assert.Equal(20, referencias.Length);
        foreach (var (punto, kmEsperado) in referencias)
        {
            Assert.InRange(Math.Abs(corredor.KmSobreVia(punto) - kmEsperado), 0, 0.001);
            Assert.InRange(corredor.DistanciaMetros(punto), 0, 0.001);
            Assert.Contains(Celda.De(punto).Id, corredor.Celdas);
        }
    }

    /// <summary>La longitud total y el fin del tramo demo se calculan del eje (336 km), y el demo empieza en el km 268.</summary>
    [Fact]
    public void Longitud_y_tramo_demo_se_derivan_del_eje_completo()
    {
        // Arrange
        using var datos = LeerGeoJson();
        var propiedades = datos.RootElement.GetProperty("properties");

        // Act
        var corredor = Crear(datos);

        // Assert
        Assert.Equal(propiedades.GetProperty("longitud_km").GetDouble(), corredor.LongitudKm, 8);
        Assert.Equal(corredor.LongitudKm, propiedades.GetProperty("demo_km_fin").GetDouble(), 8);
        Assert.Equal(268, propiedades.GetProperty("demo_km_inicio").GetDouble());
        Assert.Equal(336, Math.Ceiling(corredor.LongitudKm));
    }

    private static JsonDocument LeerGeoJson() => JsonDocument.Parse(File.ReadAllText(Ruta("corredor_pe28a.geojson")));

    /// <summary>GeoJSON guarda [longitud, latitud]: se invierten al crear la coordenada.</summary>
    private static CorredorVia Crear(JsonDocument datos) => new(datos.RootElement.GetProperty("geometry").GetProperty("coordinates")
        .EnumerateArray().Select(p => Coordenada.Crear(p[1].GetDouble(), p[0].GetDouble())!.Value).ToArray(), MargenMetros);

    /// <summary>Columnas del CSV: id, longitud, latitud, km.</summary>
    private static (Coordenada Punto, double Km) LeerReferencia(string fila)
    {
        var columnas = fila.Split(',');
        return (Coordenada.Crear(Numero(columnas[2]), Numero(columnas[1]))!.Value, Numero(columnas[3]));
    }

    private static double Numero(string valor) => double.Parse(valor, CultureInfo.InvariantCulture);

    private static string Ruta(string nombre) => Path.Combine(AppContext.BaseDirectory, "Datos", nombre);
}
