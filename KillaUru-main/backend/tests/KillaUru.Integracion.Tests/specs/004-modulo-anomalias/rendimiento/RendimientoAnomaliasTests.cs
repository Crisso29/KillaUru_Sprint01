using System.Collections.Concurrent;
using System.Text.Json;
using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Medición local del procesador con 300 dispositivos y 8 trabajadores, con reloj real. Solo
/// <b>registra</b> el p95 en TestResults/004-rendimiento.json: no exige el umbral de 100 ms porque
/// T053 se valida junto con la cola de 005. Spec 004 · SC-001, FR-031 (T053 abierta).
/// </summary>
[Collection("Anomalias")]
public sealed class RendimientoAnomaliasTests(BaseDatosPrueba bd)
{
    private const int Dispositivos = 300;
    private const int Trabajadores = 8;

    [Fact]
    public async Task Mide_300_dispositivos_con_reloj_real()
    {
        // Arrange
        var duraciones = new ConcurrentBag<double>();
        using var api = ApiQueMideDuraciones(duraciones);
        await ProcesarAsync(api.Services, 0, EscenarioAnomalias.Ct); // calienta conexiones y JIT
        duraciones.Clear();
        var paralelismo = new ParallelOptions { MaxDegreeOfParallelism = Trabajadores, CancellationToken = EscenarioAnomalias.Ct };

        // Act
        await Parallel.ForEachAsync(Enumerable.Range(1, Dispositivos), paralelismo, (i, ct) => new ValueTask(ProcesarAsync(api.Services, i, ct)));

        // Assert
        var muestras = duraciones.Order().ToArray();
        Assert.Equal(Dispositivos, muestras.Length);
        await GuardarMedicionAsync(muestras);
    }

    /// <summary>Host con métricas que anotan cada duración y una recta propia de 1° de longitud.</summary>
    private WebApplicationFactory<Program> ApiQueMideDuraciones(ConcurrentBag<double> duraciones)
    {
        var metricas = Substitute.For<IMetricas>();
        metricas.When(m => m.RegistrarDuracionProceso(Arg.Any<TimeSpan>())).Do(c => duraciones.Add(c.Arg<TimeSpan>().TotalMilliseconds));
        var corredor = new EstadoCorredor();
        corredor.Cargar(new([Coordenada.Crear(-12, -76)!.Value, Coordenada.Crear(-12, -75)!.Value], 50));
        return new FabricaApi(bd).WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddSingleton(metricas);
            s.AddSingleton(corredor);
        }));
    }

    /// <summary>Cada dispositivo reporta 220 m más al este que el anterior: ninguno se agrupa con otro.</summary>
    private static async Task ProcesarAsync(IServiceProvider servicios, int dispositivo, CancellationToken ct)
    {
        await using var scope = servicios.CreateAsyncScope();
        var ahora = TimeProvider.System.GetUtcNow();
        var evento = new EventoReportado(Guid.NewGuid(), ahora, -12, -75.95 + dispositivo * 0.002, 2, "Bache", 2, 0.9, "prueba-rendimiento", Guid.NewGuid());
        var resultado = await scope.ServiceProvider.GetRequiredService<IAnomaliasService>().ProcesarReporteAsync(evento, new(false, 1, ahora), ct);
        Assert.Null(resultado.Motivo);
        Assert.False(resultado.YaProcesado);
    }

    /// <summary>p95 = muestra 285 de 300 (índice 284).</summary>
    private static Task GuardarMedicionAsync(double[] muestras)
    {
        var p95 = muestras[(int)Math.Ceiling(muestras.Length * 0.95) - 1];
        var medicion = new { dispositivos = Dispositivos, trabajadores = Trabajadores, p95Ms = p95, maxMs = muestras[^1], mediaMs = muestras.Average(), cola005 = false };
        return File.WriteAllTextAsync(Path.Combine(Rutas.Backend, "TestResults/004-rendimiento.json"), JsonSerializer.Serialize(medicion), EscenarioAnomalias.Ct);
    }
}
