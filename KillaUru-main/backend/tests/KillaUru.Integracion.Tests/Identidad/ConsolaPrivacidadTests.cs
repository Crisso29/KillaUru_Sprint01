using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Captura el sink de Serilog con la composición real y puertos de persistencia aislados.</summary>
public sealed class ConsolaPrivacidadTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Enlace_inicial_aparece_solo_en_salida_y_no_en_sink()
    {
        using var logs = new StringWriter();
        using var salida = new StringWriter();
        using var logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(new SinkDePrueba(logs)).CreateLogger();
        var usuarios = Substitute.For<IUsuarioRepositorio>();
        usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 1, 0));
        var tx = Substitute.For<ITransaccionIdentidad>();
        tx.EjecutarAsync(Arg.Any<Func<Task<ResultadoEnlace>>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(x => x.Arg<Func<Task<ResultadoEnlace>>>()());
        using var api = new FabricaApi(bd).WithWebHostBuilder(builder => builder.ConfigureTestServices(s =>
        {
            s.AddSingleton(usuarios);
            s.AddSingleton(tx);
            s.AddSingleton(Substitute.For<ITokenCorreoRepositorio>());
            s.AddSingleton(Substitute.For<IAuditoriaService>());
            s.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory(logger, dispose: false));
        }));
        LoggerMessage.Define(LogLevel.Information, new EventId(1), "Captura activa")(
            api.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test"), null);
        Assert.Equal(0, await IdentidadModule.CrearAdministradorInicialAsync(api.Services, "semilla@example.com", salida, TestContext.Current.CancellationToken));
        var texto = salida.ToString();
        Assert.Contains("#token=", texto);
        var token = texto.Split("#token=", StringSplitOptions.None)[1].Trim();
        Assert.Contains("Captura activa", logs.ToString());
        Assert.DoesNotContain(token, logs.ToString());
        Assert.DoesNotContain("semilla@example.com", logs.ToString());
        Assert.DoesNotContain("#token=", logs.ToString());
    }
}
