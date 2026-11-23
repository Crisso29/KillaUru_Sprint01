using Serilog.Events;
using Serilog.Parsing;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// DT-04 / 001-T084: el sink que captura logs en <see cref="ConsolaPrivacidadTests"/> recibe eventos de varios hilos
/// (servicios en segundo plano del host) mientras la prueba lee lo capturado. Leer un <c>StringBuilder</c> a la vez que
/// otro hilo escribe lanza <c>ArgumentOutOfRangeException … chunkLength</c>: la falla intermitente de CI.
/// Es una prueba unitaria del soporte: no necesita base de datos.
/// </summary>
public sealed class SinkDePruebaTests
{
    private const int Hilos = 8;
    private const int EventosPorHilo = 2_000;

    private static LogEvent Evento(int numero) => new(DateTimeOffset.UnixEpoch, LogEventLevel.Information, null,
        new MessageTemplateParser().Parse($"evento {numero}"), []);

    [Fact]
    public async Task Leer_mientras_otros_hilos_escriben_no_falla_ni_pierde_eventos()
    {
        // Arrange
        var cancelacion = TestContext.Current.CancellationToken;
        var sink = new SinkDePrueba();
        var escritores = Enumerable.Range(0, Hilos).Select(hilo => Task.Run(() =>
        {
            for (var numero = 0; numero < EventosPorHilo; numero++) sink.Emit(Evento(numero));
        }, cancelacion)).ToArray();

        // Act — como la prueba real: lee lo capturado mientras el host sigue registrando
        var lector = Task.Run(() =>
        {
            while (!escritores.All(escritor => escritor.IsCompleted)) _ = sink.Capturado;
        }, cancelacion);
        await Task.WhenAll([.. escritores, lector]);

        // Assert — una línea JSON por evento
        Assert.Equal(Hilos * EventosPorHilo, sink.Capturado.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
    }
}
