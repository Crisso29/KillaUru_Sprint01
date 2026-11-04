using KillaUru.Dashboard.Application;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Dashboard.Tests.Application;

/// <summary>T016: ConsultarResumenUseCase lee dias×2+7 días y construye los EventosPorDia.</summary>
public sealed class ConsultarResumenUseCaseTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 23, 10, 0, 0, TimeSpan.Zero);

    private static ConsultarResumenUseCase Crear(IIndicadoresRepositorio repo, CalculadoUtcHolder? holder = null)
    {
        var opts  = Options.Create(new DashboardOptions { DesfaseHorarioHoras = -5 });
        var reloj = new FakeTimeProvider(Ahora);
        return new ConsultarResumenUseCase(repo, holder ?? new CalculadoUtcHolder(), opts, reloj);
    }

    [Fact]
    public async Task Lee_exactamente_dias_por_dos_mas_siete_dias()
    {
        // Arrange
        var repo = Substitute.For<IIndicadoresRepositorio>();
        repo.LeerAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);
        var uc = Crear(repo);

        // Act
        await uc.EjecutarAsync(dias: 7, CancellationToken.None);

        // Assert — debe leer 7×2+7=21 días (desde D-20 hasta D)
        var d        = new DateOnly(2026, 10, 22);   // ayer
        var esperado = d.AddDays(-(7 * 2 + 7 - 1)); // D-20
        await repo.Received(1).LeerAsync(
            Arg.Is<DateOnly>(x => x == esperado),
            Arg.Is<DateOnly>(x => x == d),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sin_datos_devuelve_indicadores_null()
    {
        // Arrange
        var repo = Substitute.For<IIndicadoresRepositorio>();
        repo.LeerAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);
        var uc = Crear(repo);

        // Act
        var r = await uc.EjecutarAsync(7, CancellationToken.None);

        // Assert
        Assert.Null(r.UltimoDiaCerrado);
        Assert.Null(r.EventosDia.Valor);
        Assert.Empty(r.EventosPorDia);
    }

    [Fact]
    public async Task CalculadoUtc_viene_del_holder()
    {
        // Arrange
        var repo   = Substitute.For<IIndicadoresRepositorio>();
        repo.LeerAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns([]);
        var holder = new CalculadoUtcHolder();
        holder.Actualizar(Ahora);
        var uc = Crear(repo, holder);

        // Act
        var r = await uc.EjecutarAsync(7, CancellationToken.None);

        // Assert
        Assert.Equal(Ahora, r.CalculadoUtc);
    }

    [Fact]
    public async Task EventosPorDia_tiene_tantos_elementos_como_dias()
    {
        // Arrange
        var repo = Substitute.For<IIndicadoresRepositorio>();
        // Proporcionar 14 días de datos para que haya un ultDia
        var d14 = Enumerable.Range(0, 14)
            .Select<int, DiaIndicadores?>(i => new DiaIndicadores(
                new DateOnly(2026, 10, 9).AddDays(i), 100, 40, 10, 5, 10, 5, 2, 1, 1))
            .ToList();
        repo.LeerAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(d14);
        var uc = Crear(repo);

        // Act
        var r = await uc.EjecutarAsync(7, CancellationToken.None);

        // Assert
        Assert.Equal(7, r.EventosPorDia.Count);
    }
}
