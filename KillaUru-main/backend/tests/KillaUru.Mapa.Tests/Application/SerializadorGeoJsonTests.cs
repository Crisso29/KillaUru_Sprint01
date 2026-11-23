using System.Text.Json;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Mapa.Application;

namespace KillaUru.Mapa.Tests.Application;

/// <summary>
/// El mapa se arma uniendo fragmentos de celdas ya serializados (caché por celda, FR-007). Lo que sale debe ser
/// GeoJSON válido: la app y el panel lo leen con un parser estricto. Encontrado con la colección de Postman
/// (2026-11-23): una celda con 2 o más anomalías producía «},,{» y el mapa entero era ilegible.
/// Spec 006 · data-model §2.
/// </summary>
public sealed class SerializadorGeoJsonTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 23, 15, 0, 0, TimeSpan.Zero);

    private static AnomaliaVisible Anomalia(double longitud) => new(Guid.NewGuid(), TipoAnomalia.Bache, 3, "Confirmada",
        -13.16, longitud, 268.5f, 1L, Ahora.AddHours(-1), Ahora.AddDays(3));

    private static JsonElement Features(byte[] coleccion) =>
        JsonDocument.Parse(coleccion).RootElement.GetProperty("features");

    [Fact]
    public void Una_celda_con_dos_anomalias_da_un_GeoJSON_valido()
    {
        // Arrange
        var celda = SerializadorGeoJson.SerializarCelda([Anomalia(-74.20), Anomalia(-74.21)]);

        // Act
        var coleccion = SerializadorGeoJson.EnsamblarColeccion([celda], Ahora);

        // Assert — JsonDocument.Parse lanza si hay comas dobles
        Assert.Equal(2, Features(coleccion).GetArrayLength());
    }

    [Fact]
    public void Varias_celdas_y_una_vacia_suman_todas_sus_anomalias()
    {
        // Arrange — la celda vacía no debe dejar una coma suelta
        var celdas = new[]
        {
            SerializadorGeoJson.SerializarCelda([Anomalia(-74.20)]),
            SerializadorGeoJson.SerializarCelda([]),
            SerializadorGeoJson.SerializarCelda([Anomalia(-74.22), Anomalia(-74.23), Anomalia(-74.24)]),
        };

        // Act
        var coleccion = SerializadorGeoJson.EnsamblarColeccion(celdas, Ahora);

        // Assert
        Assert.Equal(4, Features(coleccion).GetArrayLength());
    }

    [Fact]
    public void Las_coordenadas_van_en_orden_longitud_latitud()
    {
        // Arrange — RFC 7946: [lon, lat]; al revés el bache aparecería en otro continente
        var celda = SerializadorGeoJson.SerializarCelda([Anomalia(-74.20)]);

        // Act
        var coordenadas = Features(SerializadorGeoJson.EnsamblarColeccion([celda], Ahora))[0]
            .GetProperty("geometry").GetProperty("coordinates");

        // Assert
        Assert.Equal((-74.20, -13.16), (coordenadas[0].GetDouble(), coordenadas[1].GetDouble()));
    }
}
