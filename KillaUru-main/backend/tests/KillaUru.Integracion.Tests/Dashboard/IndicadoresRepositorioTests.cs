using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Dashboard;

/// <summary>T010: IndicadoresRepositorio sobre PostgreSQL real.</summary>
public sealed class IndicadoresRepositorioTests(BaseDatosPrueba bd)
{
    private static readonly DateOnly Fecha = new(2025, 11, 1);

    private static DiaIndicadores Dia(DateOnly d) =>
        new(d, 500L, 42L, 15L, 237, 336, 18L, 6L, 3L, 2L);

    [Fact]
    public async Task GuardarDia_persiste_las_9_filas()
    {
        // Arrange
        using var escenario  = new EscenarioDashboard(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var repo             = scope.ServiceProvider.GetRequiredService<IIndicadoresRepositorio>();

        // Act
        await repo.GuardarDiaAsync(Dia(Fecha), EscenarioDashboard.Ct);

        // Assert
        var completos = await repo.DiasCompletosAsync(Fecha, Fecha, EscenarioDashboard.Ct);
        Assert.Contains(Fecha, completos);
    }

    [Fact]
    public async Task GuardarDia_es_idempotente()
    {
        // Arrange
        var dia   = Fecha.AddDays(1);
        using var escenario  = new EscenarioDashboard(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var repo             = scope.ServiceProvider.GetRequiredService<IIndicadoresRepositorio>();
        await repo.GuardarDiaAsync(Dia(dia), EscenarioDashboard.Ct);

        // Act — segunda escritura del mismo día
        await repo.GuardarDiaAsync(Dia(dia), EscenarioDashboard.Ct);

        // Assert — sigue habiendo exactamente 9 filas (ON CONFLICT DO NOTHING)
        var datos = await repo.LeerAsync(dia, dia, EscenarioDashboard.Ct);
        Assert.NotNull(datos[0]);
    }

    [Fact]
    public async Task LeerAsync_devuelve_null_para_dias_sin_datos()
    {
        // Arrange
        var diaVacio = Fecha.AddDays(2);
        var diaLleno = Fecha.AddDays(3);
        using var escenario  = new EscenarioDashboard(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var repo             = scope.ServiceProvider.GetRequiredService<IIndicadoresRepositorio>();
        await repo.GuardarDiaAsync(Dia(diaLleno), EscenarioDashboard.Ct);

        // Act
        var datos = await repo.LeerAsync(diaVacio, diaLleno, EscenarioDashboard.Ct);

        // Assert
        Assert.Equal(2,    datos.Count);
        Assert.Null(datos[0]);      // diaVacio
        Assert.NotNull(datos[1]);   // diaLleno
    }

    [Fact]
    public async Task PurgarAsync_elimina_filas_antiguas()
    {
        // Arrange
        var antigua = Fecha.AddDays(4);
        using var escenario  = new EscenarioDashboard(bd);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var repo             = scope.ServiceProvider.GetRequiredService<IIndicadoresRepositorio>();
        await repo.GuardarDiaAsync(Dia(antigua), EscenarioDashboard.Ct);

        // Act
        await repo.PurgarAsync(antigua.AddDays(1), EscenarioDashboard.Ct);

        // Assert
        var datos = await repo.LeerAsync(antigua, antigua, EscenarioDashboard.Ct);
        Assert.Null(datos[0]);
    }
}
