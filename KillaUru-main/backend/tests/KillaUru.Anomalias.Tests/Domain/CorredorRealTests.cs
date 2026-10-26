using System.Globalization;
using System.Text.Json;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>Protege el kilometraje compartido con Android y evita recortar la vía a una longitud inventada.</summary>
public sealed class CorredorRealTests
{
    [Fact]
    public void Veinte_referencias_compartidas_coinciden_a_menos_de_un_metro()
    {
        using var datos = JsonDocument.Parse(File.ReadAllText(Ruta("corredor_pe28a.geojson")));
        var corredor = Crear(datos);
        var filas = File.ReadAllLines(Ruta("km_referencia.csv")).Skip(1).ToArray();
        Assert.Equal(20, filas.Length);
        foreach (var fila in filas)
        {
            var columnas = fila.Split(',');
            var punto = Coordenada.Crear(Numero(columnas[2]), Numero(columnas[1]))!.Value;
            Assert.InRange(Math.Abs(corredor.KmSobreVia(punto) - Numero(columnas[3])), 0, 0.001);
            Assert.InRange(corredor.DistanciaMetros(punto), 0, 0.001);
            Assert.Contains(Celda.De(punto).Id, corredor.Celdas);
        }
    }

    [Fact]
    public void Longitud_y_demo_se_derivan_del_eje_completo()
    {
        using var datos = JsonDocument.Parse(File.ReadAllText(Ruta("corredor_pe28a.geojson")));
        var corredor = Crear(datos);
        var propiedades = datos.RootElement.GetProperty("properties");
        Assert.Equal(propiedades.GetProperty("longitud_km").GetDouble(), corredor.LongitudKm, 8);
        Assert.Equal(corredor.LongitudKm, propiedades.GetProperty("demo_km_fin").GetDouble(), 8);
        Assert.Equal(268, propiedades.GetProperty("demo_km_inicio").GetDouble());
        Assert.Equal(336, Math.Ceiling(corredor.LongitudKm));
    }

    private static CorredorVia Crear(JsonDocument datos) => new(datos.RootElement.GetProperty("geometry")
        .GetProperty("coordinates").EnumerateArray().Select(p => Coordenada.Crear(p[1].GetDouble(), p[0].GetDouble())!.Value).ToArray(), 50);
    private static double Numero(string valor) => double.Parse(valor, CultureInfo.InvariantCulture);
    private static string Ruta(string nombre) => Path.Combine(AppContext.BaseDirectory, "Datos", nombre);
}
