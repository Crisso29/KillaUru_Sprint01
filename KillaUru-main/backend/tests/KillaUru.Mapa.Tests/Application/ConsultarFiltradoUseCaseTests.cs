using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Mapa.Application;
using KillaUru.Mapa.Domain;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Mapa.Tests.Application;

/// <summary>
/// ConsultarFiltradoUseCase llama FiltrarAsync y aplica el recorte demo para el anónimo.
/// Spec 006 · FR-010, FR-011, FR-004. Scenario 6–8 de quickstart.
/// </summary>
public sealed class ConsultarFiltradoUseCaseTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 15, 02, 11, TimeSpan.Zero);

    private static AnomaliaVisible AnomaliaEn(double lat, double lon) =>
        new(Guid.NewGuid(), TipoAnomalia.Bache, 4, "Persistente",
            lat, lon, 287.4f, 1_799_825_000L, Ahora.AddHours(-1), Ahora.AddDays(7));

    private static (ConsultarFiltradoUseCase uc, IAnomaliaLectura anomalias) Crear(ZonaDemo? zona = null)
    {
        var anomalias = Substitute.For<IAnomaliaLectura>();
        var reloj = new FakeTimeProvider();
        reloj.SetUtcNow(Ahora);

        // Polígono que cubre todo → Dentro para cualquier punto
        zona ??= new ZonaDemo(new PoligonoGeo([[-180.0, -90.0], [180.0, -90.0], [180.0, 90.0], [-180.0, 90.0], [-180.0, -90.0]]));
        var uc = new ConsultarFiltradoUseCase(anomalias, zona, reloj);
        return (uc, anomalias);
    }

    /// <summary>Devuelve Contenido con el cuerpo serializado de los filtros.</summary>
    [Fact]
    public async Task Devuelve_Contenido_sin_ETag()
    {
        // Arrange
        var (uc, anomalias) = Crear();
        anomalias.FiltrarAsync(Arg.Any<FiltroAnomalias>(), Arg.Any<CancellationToken>())
                 .Returns(new List<AnomaliaCambio>());

        // Act
        var resultado = await uc.EjecutarAsync(null, null, null, null, null,
            [KillaUru.Anomalias.Contracts.EstadoAnomalia.Activa], recortarDemo: false, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        Assert.Null(contenido.ETag);
        Assert.NotEmpty(contenido.Cuerpo);
    }

    /// <summary>Con recortarDemo=true filtra punto a punto los que no están dentro del polígono.</summary>
    [Fact]
    public async Task RecortarDemo_filtra_anomalias_fuera_del_poligono()
    {
        // Arrange — polígono pequeño que solo cubre (0,0)
        var zonaChica = new ZonaDemo(new PoligonoGeo([[-1.0, -1.0], [1.0, -1.0], [1.0, 1.0], [-1.0, 1.0], [-1.0, -1.0]]));
        var (uc, anomalias) = Crear(zonaChica);

        var dentro  = new AnomaliaCambio(Guid.NewGuid(), TipoAnomalia.Bache, 4, "Persistente",
            0.0, 0.0, 287.4f, 1L, Ahora.AddHours(-1), Ahora.AddDays(7),
            KillaUru.Anomalias.Contracts.EstadoAnomalia.Activa, Ahora.AddHours(-1));
        var fuera = new AnomaliaCambio(Guid.NewGuid(), TipoAnomalia.Bache, 4, "Persistente",
            5.0, 5.0, 300.0f, 2L, Ahora.AddHours(-1), Ahora.AddDays(7),
            KillaUru.Anomalias.Contracts.EstadoAnomalia.Activa, Ahora.AddHours(-1));

        anomalias.FiltrarAsync(Arg.Any<FiltroAnomalias>(), Arg.Any<CancellationToken>())
                 .Returns([dentro, fuera]);

        // Act
        var resultado = await uc.EjecutarAsync(null, null, null, null, null,
            [KillaUru.Anomalias.Contracts.EstadoAnomalia.Activa], recortarDemo: true, CancellationToken.None);

        // Assert — solo la anomalía dentro del polígono
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.Contains(dentro.Id.ToString(), json);
        Assert.DoesNotContain(fuera.Id.ToString(), json);
    }

    /// <summary>No hay anomalías → FeatureCollection vacía con generadoUtc.</summary>
    [Fact]
    public async Task Sin_anomalias_devuelve_coleccion_vacia_con_generadoUtc()
    {
        // Arrange
        var (uc, anomalias) = Crear();
        anomalias.FiltrarAsync(Arg.Any<FiltroAnomalias>(), Arg.Any<CancellationToken>())
                 .Returns(new List<AnomaliaCambio>());

        // Act
        var resultado = await uc.EjecutarAsync(null, null, null, null, null,
            [KillaUru.Anomalias.Contracts.EstadoAnomalia.Activa], recortarDemo: false, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.Contains("generadoUtc", json);
        Assert.Contains("FeatureCollection", json);
    }
}
