using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// PostGIS elige la anomalía más cercana del mismo tipo dentro de 100 m, aunque haya varias
/// candidatas. Todo ocurre en una transacción que se deshace al final. Spec 004 · FR-008, FR-030.
/// </summary>
[Collection("Anomalias")]
public sealed class RepositorioEspacialTests(BaseDatosPrueba bd)
{
    private const int RadioMetros = 100;

    /// <summary>Tres anomalías en la latitud −10: dos baches a ~165 m entre sí y un hundimiento.</summary>
    /// <remarks>Al liberarse deshace la transacción: la base queda como estaba.</remarks>
    private sealed record Vecindario(IAnomaliaRepository Repositorio, Anomalia BacheOeste, Anomalia Hundimiento, IDbContextTransaction Transaccion)
        : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => Transaccion.DisposeAsync();
    }

    private static readonly Coordenada EntreAmbosBaches = Coordenada.Crear(-10, -69.9994)!.Value;
    private static readonly Coordenada Lejos = Coordenada.Crear(-10, -69.99)!.Value;

    private static async Task<Vecindario> SembrarAsync(IServiceProvider servicios)
    {
        var db = servicios.GetRequiredService<AnomaliasDbContext>();
        var repositorio = servicios.GetRequiredService<IAnomaliaRepository>();
        var transaccion = await db.Database.BeginTransactionAsync(EscenarioAnomalias.Ct);
        var (bacheOeste, bacheEste, hundimiento) = (Crear(-70, "Bache"), Crear(-69.9985, "Bache"), Crear(-70, "Hundimiento"));
        foreach (var anomalia in new[] { bacheOeste, bacheEste, hundimiento }) await repositorio.GuardarAsync(anomalia, true, EscenarioAnomalias.Ct);
        return new(repositorio, bacheOeste, hundimiento, transaccion);
    }

    private static Anomalia Crear(double longitud, string tipo)
    {
        var ahora = TimeProvider.System.GetUtcNow();
        var reporte = ReporteAnomalia.Desde(new(Guid.NewGuid(), ahora, -10, longitud, 1, tipo, 2, 0.9, "prueba", Guid.NewGuid()), new(false, 1, ahora)).Reporte!;
        var anomalia = Anomalia.Crear(reporte, false, ahora);
        anomalia.RegistrarReporte(reporte, [reporte], new(), ahora);
        return anomalia;
    }

    /// <summary>
    /// Entre dos baches a 66 m y 99 m gana el más cercano; el hundimiento a la misma distancia no
    /// cuenta para un bache; a ~1 km no hay ninguna. La fila se puede releer con bloqueo.
    /// </summary>
    [Fact]
    public async Task Vecino_mas_cercano_del_mismo_tipo_y_dentro_del_radio()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        await using var scope = api.Services.CreateAsyncScope();
        await using var vecindario = await SembrarAsync(scope.ServiceProvider);
        var repositorio = vecindario.Repositorio;

        // Act
        var bache = await repositorio.MasCercanaAsync(EntreAmbosBaches, TipoAnomalia.Bache, RadioMetros, EscenarioAnomalias.Ct);
        var hundimiento = await repositorio.MasCercanaAsync(EntreAmbosBaches, TipoAnomalia.Hundimiento, RadioMetros, EscenarioAnomalias.Ct);
        var ninguna = await repositorio.MasCercanaAsync(Lejos, TipoAnomalia.Bache, RadioMetros, EscenarioAnomalias.Ct);
        var bloqueada = await repositorio.ObtenerParaActualizarAsync(vecindario.BacheOeste.Id, EscenarioAnomalias.Ct);

        // Assert
        Assert.Equal(vecindario.BacheOeste.Id, bache!.Id);
        Assert.Equal(vecindario.Hundimiento.Id, hundimiento!.Id);
        Assert.Null(ninguna);
        Assert.Equal(vecindario.BacheOeste.Id, bloqueada!.Id);
    }
}
