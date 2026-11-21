using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// El enlace del primer administrador es una credencial: se imprime solo en la consola del comando
/// y nunca en los logs, que viajan a Grafana Cloud. Se usa la composición real del host con el
/// sink de Serilog capturado y la persistencia sustituida. Spec 003 · FR-043.
/// </summary>
public sealed class ConsolaPrivacidadTests(BaseDatosPrueba bd)
{
    /// <summary>Host real con logs capturados y una base sin administradores.</summary>
    private WebApplicationFactory<Program> HostConLogsCapturados(Serilog.ILogger logger)
    {
        var usuarios = Substitute.For<IUsuarioRepositorio>();
        usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 1, 0));
        var transaccion = Substitute.For<ITransaccionIdentidad>();
        transaccion.EjecutarAsync(Arg.Any<Func<Task<ResultadoEnlace>>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(llamada => llamada.Arg<Func<Task<ResultadoEnlace>>>()());
        return new FabricaApi(bd).WithWebHostBuilder(builder => builder.ConfigureTestServices(s =>
        {
            s.AddSingleton(usuarios);
            s.AddSingleton(transaccion);
            s.AddSingleton(Substitute.For<ITokenCorreoRepositorio>());
            s.AddSingleton(Substitute.For<IAuditoriaService>());
            s.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory(logger, dispose: false));
        }));
    }

    /// <summary>Una línea conocida demuestra que el sink sí captura; si no, la ausencia del token no probaría nada.</summary>
    private static void EscribirLineaDeControl(IServiceProvider servicios) =>
        LoggerMessage.Define(LogLevel.Information, new EventId(1), "Captura activa")(
            servicios.GetRequiredService<ILoggerFactory>().CreateLogger("Test"), null);

    [Fact]
    public async Task Enlace_inicial_aparece_solo_en_la_consola_y_no_en_los_logs()
    {
        // Arrange
        using var logs = new StringWriter();
        using var consola = new StringWriter();
        using var logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(new SinkDePrueba(logs)).CreateLogger();
        using var api = HostConLogsCapturados(logger);
        EscribirLineaDeControl(api.Services);

        // Act
        var codigoSalida = await IdentidadModule.CrearAdministradorInicialAsync(api.Services, "semilla@example.com", consola, EscenarioIdentidad.Ct);

        // Assert
        Assert.Equal(0, codigoSalida);
        var token = consola.ToString().Split("#token=", StringSplitOptions.None)[1].Trim();
        var registrado = logs.ToString();
        Assert.Contains("Captura activa", registrado);
        Assert.All(new[] { token, "semilla@example.com", "#token=" }, secreto => Assert.DoesNotContain(secreto, registrado));
    }
}
