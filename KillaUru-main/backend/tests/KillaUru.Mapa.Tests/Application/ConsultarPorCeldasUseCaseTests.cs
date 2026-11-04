using KillaUru.Anomalias.Contracts;
using KillaUru.Mapa.Application;
using KillaUru.Mapa.Application.Puertos;
using KillaUru.Mapa.Domain;
using KillaUru.Shared.Domain;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Mapa.Tests.Application;

/// <summary>
/// ConsultarPorCeldasUseCase resuelve el mapa por celdas con caché y ETag.
/// Spec 006 · FR-006, FR-007, FR-008, FR-009. Scenario 2 y 3 de quickstart.
/// </summary>
public sealed class ConsultarPorCeldasUseCaseTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 15, 02, 11, TimeSpan.Zero);
    private static readonly long CeldaId = 1_799_825_000L;

    private static (ConsultarPorCeldasUseCase uc,
        IFuenteCeldas fuente, IVersionesCelda versiones, IMetricasMapa metricas)
    Crear(IReadOnlySet<long>? celdas = null)
    {
        var anomalias = Substitute.For<IAnomaliaLectura>();
        anomalias.CeldasDelCorredor().Returns(celdas ?? new HashSet<long> { CeldaId });

        var fuente    = Substitute.For<IFuenteCeldas>();
        var versiones = Substitute.For<IVersionesCelda>();
        var metricas  = Substitute.For<IMetricasMapa>();
        var reloj     = new FakeTimeProvider();
        reloj.SetUtcNow(Ahora);
        versiones.InstanteArranque.Returns(Ahora.AddMinutes(-5));
        versiones.ObtenerVersion(Arg.Any<long>()).Returns(0L);

        var zona      = new ZonaDemo(new PoligonoGeo([[-180.0, -90.0], [180.0, -90.0], [180.0, 90.0], [-180.0, 90.0], [-180.0, -90.0]]));
        var corredor  = new CorredorMapa(anomalias, zona);
        var calculador = new CalculadorETag(versiones);
        var uc = new ConsultarPorCeldasUseCase(corredor, fuente, calculador, metricas, reloj);
        return (uc, fuente, versiones, metricas);
    }

    /// <summary>ETag coincide con If-None-Match → 304 NoModificado sin consultar fuente.</summary>
    [Fact]
    public async Task ETag_coincide_devuelve_NoModificado_sin_consultar_fuente()
    {
        // Arrange
        var (uc, fuente, versiones, metricas) = Crear();
        versiones.InstanteArranque.Returns(Ahora.AddMinutes(-5));
        versiones.ObtenerVersion(CeldaId).Returns(0L);

        var primeraRespuesta = await uc.EjecutarAsync(Alcance.Registrado, null, null, CancellationToken.None);
        var etag = ((ResultadoMapa.Contenido)primeraRespuesta).ETag!;

        // Act
        var segunda = await uc.EjecutarAsync(Alcance.Registrado, null, etag, CancellationToken.None);

        // Assert
        Assert.IsType<ResultadoMapa.NoModificado>(segunda);
        await fuente.Received(1).ObtenerAsync(Arg.Any<long>(), Arg.Any<Alcance>(), Arg.Any<CancellationToken>());
        metricas.Received().RegistrarCache("no_modificado");
    }

    /// <summary>Sin If-None-Match → 200 Contenido con cuerpo y ETag.</summary>
    [Fact]
    public async Task Sin_ifNoneMatch_devuelve_Contenido_con_cuerpo_y_ETag()
    {
        // Arrange
        var (uc, fuente, _, _) = Crear();
        fuente.ObtenerAsync(CeldaId, Alcance.Registrado, Arg.Any<CancellationToken>())
              .Returns(Task.FromResult("{}"u8.ToArray()));

        // Act
        var resultado = await uc.EjecutarAsync(Alcance.Registrado, null, null, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        Assert.NotNull(contenido.ETag);
        Assert.NotEmpty(contenido.Cuerpo);
    }

    /// <summary>Alcance Anonimo → Publico = true.</summary>
    [Fact]
    public async Task Alcance_Anonimo_devuelve_Publico_true()
    {
        // Arrange
        var (uc, fuente, _, _) = Crear();
        fuente.ObtenerAsync(CeldaId, Alcance.Anonimo, Arg.Any<CancellationToken>())
              .Returns(Task.FromResult(Array.Empty<byte>()));

        // Act
        var resultado = await uc.EjecutarAsync(Alcance.Anonimo, null, null, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        Assert.True(contenido.Publico);
    }

    /// <summary>Alcance Registrado → Publico = false.</summary>
    [Fact]
    public async Task Alcance_Registrado_devuelve_Publico_false()
    {
        // Arrange
        var (uc, fuente, _, _) = Crear();
        fuente.ObtenerAsync(CeldaId, Alcance.Registrado, Arg.Any<CancellationToken>())
              .Returns(Task.FromResult(Array.Empty<byte>()));

        // Act
        var resultado = await uc.EjecutarAsync(Alcance.Registrado, null, null, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        Assert.False(contenido.Publico);
    }

    /// <summary>Corredor vacío → colección sin features, cuerpo no vacío (wrapper válido).</summary>
    [Fact]
    public async Task Corredor_vacio_devuelve_coleccion_vacia()
    {
        // Arrange
        var (uc, _, _, _) = Crear(new HashSet<long>());

        // Act
        var resultado = await uc.EjecutarAsync(Alcance.Registrado, null, null, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapa.Contenido>(resultado);
        Assert.NotEmpty(contenido.Cuerpo);
    }
}
