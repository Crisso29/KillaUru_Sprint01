using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Mapa.Application;
using KillaUru.Mapa.Domain;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Mapa.Tests.Application;

/// <summary>
/// ConsultarCambiosUseCase separa altas y bajas, aplica el recorte demo para el anónimo.
/// Spec 006 · FR-012, FR-013. Scenario 9 de quickstart; R-012.
/// </summary>
public sealed class ConsultarCambiosUseCaseTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 15, 02, 11, TimeSpan.Zero);
    private static readonly DateTimeOffset Since  = Ahora.AddHours(-1);

    private static AnomaliaCambio Cambio(EstadoAnomalia estado, double lat = 0.0, double lon = 0.0) =>
        new(Guid.NewGuid(), TipoAnomalia.Bache, 4, "Persistente",
            lat, lon, 287.4f, 1L, Ahora.AddMinutes(-30), Ahora.AddDays(7), estado, Ahora.AddMinutes(-15));

    private static (ConsultarCambiosUseCase uc, IAnomaliaLectura anomalias) Crear(ZonaDemo? zona = null)
    {
        var anomalias = Substitute.For<IAnomaliaLectura>();
        var reloj     = new FakeTimeProvider();
        reloj.SetUtcNow(Ahora);
        zona ??= new ZonaDemo(new PoligonoGeo([[-180.0, -90.0], [180.0, -90.0], [180.0, 90.0], [-180.0, 90.0], [-180.0, -90.0]]));
        var uc = new ConsultarCambiosUseCase(anomalias, zona, reloj);
        return (uc, anomalias);
    }

    /// <summary>Anomalía Activa → aparece como feature normal (alta).</summary>
    [Fact]
    public async Task Activa_aparece_como_alta_sin_eliminada()
    {
        // Arrange
        var (uc, anomalias) = Crear();
        var cambio = Cambio(EstadoAnomalia.Activa);
        anomalias.CambiosDesdeAsync(Since, Arg.Any<CancellationToken>()).Returns([cambio]);

        // Act
        var resultado = await uc.EjecutarAsync(Since, recortarDemo: false, esAdmin: false, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapaContenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.Contains(cambio.Id.ToString(), json);
        Assert.DoesNotContain("eliminada", json);
    }

    /// <summary>Anomalía no Activa (Expirada) → aparece como baja con «eliminada: true».</summary>
    [Fact]
    public async Task No_activa_aparece_como_baja_con_eliminada_true()
    {
        // Arrange
        var (uc, anomalias) = Crear();
        var cambio = Cambio(EstadoAnomalia.Expirada);
        anomalias.CambiosDesdeAsync(Since, Arg.Any<CancellationToken>()).Returns([cambio]);

        // Act
        var resultado = await uc.EjecutarAsync(Since, recortarDemo: false, esAdmin: false, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapaContenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.Contains(cambio.Id.ToString(), json);
        Assert.Contains("\"eliminada\":true", json);
    }

    /// <summary>Anónimo (recortarDemo=true): baja fuera del demo no aparece (T-SEG16; R-012).</summary>
    [Fact]
    public async Task Anonimo_baja_fuera_del_demo_no_aparece()
    {
        // Arrange — polígono que solo cubre (0,0)
        var zonaChica = new ZonaDemo(new PoligonoGeo([[-1.0, -1.0], [1.0, -1.0], [1.0, 1.0], [-1.0, 1.0], [-1.0, -1.0]]));
        var (uc, anomalias) = Crear(zonaChica);

        var bajaFuera = Cambio(EstadoAnomalia.Expirada, lat: 5.0, lon: 5.0);
        anomalias.CambiosDesdeAsync(Since, Arg.Any<CancellationToken>()).Returns([bajaFuera]);

        // Act
        var resultado = await uc.EjecutarAsync(Since, recortarDemo: true, esAdmin: false, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapaContenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.DoesNotContain(bajaFuera.Id.ToString(), json);
    }

    /// <summary>Toda respuesta incluye generadoUtc (R-006).</summary>
    [Fact]
    public async Task Respuesta_incluye_generadoUtc()
    {
        // Arrange
        var (uc, anomalias) = Crear();
        anomalias.CambiosDesdeAsync(Since, Arg.Any<CancellationToken>()).Returns([]);

        // Act
        var resultado = await uc.EjecutarAsync(Since, recortarDemo: false, esAdmin: false, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapaContenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.Contains("generadoUtc", json);
    }

    /// <summary>Admin (esAdmin=true): todas las anomalías aparecen como altas con estado.</summary>
    [Fact]
    public async Task Admin_todas_las_anomalias_son_altas_con_estado()
    {
        // Arrange
        var (uc, anomalias) = Crear();
        var activa   = Cambio(EstadoAnomalia.Activa);
        var expirada = Cambio(EstadoAnomalia.Expirada);
        anomalias.CambiosDesdeAsync(Since, Arg.Any<CancellationToken>()).Returns([activa, expirada]);

        // Act
        var resultado = await uc.EjecutarAsync(Since, recortarDemo: false, esAdmin: true, CancellationToken.None);

        // Assert
        var contenido = Assert.IsType<ResultadoMapaContenido>(resultado);
        var json = System.Text.Encoding.UTF8.GetString(contenido.Cuerpo);
        Assert.Contains(activa.Id.ToString(), json);
        Assert.Contains(expirada.Id.ToString(), json);
        Assert.DoesNotContain("\"eliminada\":true", json);
    }
}
