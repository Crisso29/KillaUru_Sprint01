using KillaUru.Identidad.Adapters.Consola;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Shared.Tareas;
using NSubstitute;
namespace KillaUru.Identidad.Tests.Application;

public sealed class ConsolaYProgramaTests
{
    [Fact]
    public async Task Primer_administrador_solo_imprime_en_la_salida_del_comando()
    {
        var e = new EscenarioAplicacion();
        e.Usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 1, 0));
        var crear = new CrearAdministradorInicialUseCase(e.Usuarios, e.Transaccion, e.Enlaces, e.Auditar, e.Reloj);
        var comando = new CrearAdminComando(crear);
        using var salida = new StringWriter();
        Assert.Equal(0, await comando.EjecutarAsync("admin@example.com", salida, TestContext.Current.CancellationToken));
        Assert.Contains("#token=secreto-sintetico", salida.ToString());
        Assert.Empty(e.Correo.ReceivedCalls());
        e.Transaccion.DidNotReceive().AlConfirmar(Arg.Any<Func<Task>>());
        e.Usuarios.ListarAsync(Arg.Any<FiltrosUsuarios>(), Arg.Any<CancellationToken>()).Returns(new PaginaUsuarios([], 1, 1, 1));
        Assert.Equal("administrador_existente", (await crear.EjecutarAsync("otro@example.com", TestContext.Current.CancellationToken)).Error);
    }

    [Fact]
    public void Purga_por_reloj_usa_0345_local_y_no_24_horas_desde_arranque()
    {
        var reloj = new Microsoft.Extensions.Time.Testing.FakeTimeProvider(new DateTimeOffset(2026, 10, 23, 8, 44, 59, TimeSpan.Zero));
        var hora = new IdentidadOptions().HoraPurgaIdentidad;
        Assert.Equal(reloj.GetUtcNow().AddSeconds(1), ProgramaReloj.ProximaDiaria(reloj.GetUtcNow(), hora));
        reloj.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(reloj.GetUtcNow().AddDays(1), ProgramaReloj.ProximaDiaria(reloj.GetUtcNow(), hora));
    }
}
