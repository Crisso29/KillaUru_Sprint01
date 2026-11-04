using KillaUru.Shared.Tareas;
using Microsoft.Extensions.Time.Testing;

namespace KillaUru.Ingesta.Tests.Infrastructure;

/// <summary>
/// Verifica que ProgramaReloj calcule correctamente los próximos instantes para las tareas
/// de purga de Ingesta: horaria en el minuto :10 y diaria a las 03:00 Lima (FR-017, FR-018, T-OPS05).
/// Spec 005 · T039.
/// </summary>
public sealed class TareasPorRelojTests
{
    private static readonly TimeOnly TresDeLaManana = new(3, 0);

    /// <summary>La purga de seudónimos se programa para el próximo :10 cuando aún no llegamos a ese minuto.</summary>
    [Fact]
    public void Purga_seudonimos_programa_siguiente_minuto_diez()
    {
        // Arrange
        var ahora = new DateTimeOffset(2026, 10, 20, 14, 5, 0, TimeSpan.Zero);  // 14:05Z

        // Act
        var proxima = ProgramaReloj.ProximaHoraria(ahora, 10);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 14, 10, 0, TimeSpan.Zero), proxima);
    }

    /// <summary>Si ya pasamos el :10, se va a la siguiente hora.</summary>
    [Fact]
    public void Purga_seudonimos_pasa_a_la_siguiente_hora_si_ya_pasamos_el_minuto()
    {
        // Arrange
        var ahora = new DateTimeOffset(2026, 10, 20, 14, 15, 0, TimeSpan.Zero);  // 14:15Z

        // Act
        var proxima = ProgramaReloj.ProximaHoraria(ahora, 10);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 15, 10, 0, TimeSpan.Zero), proxima);
    }

    /// <summary>La purga de retención se programa a las 03:00 Lima (UTC-5) = 08:00Z del mismo día si aún no llega.</summary>
    [Fact]
    public void Purga_retencion_se_programa_a_las_tres_de_lima()
    {
        // Arrange  — son las 02:00 Lima = 07:00Z
        var ahora = new DateTimeOffset(2026, 10, 20, 7, 0, 0, TimeSpan.Zero);

        // Act
        var proxima = ProgramaReloj.ProximaDiaria(ahora, TresDeLaManana);

        // Assert  — 03:00 Lima = 08:00Z del mismo día
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero), proxima);
    }

    /// <summary>Si ya son las 03:00 Lima o más, se va al día siguiente.</summary>
    [Fact]
    public void Purga_retencion_pasa_al_dia_siguiente_si_ya_paso_la_hora()
    {
        // Arrange  — son las 04:00 Lima = 09:00Z
        var ahora = new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);

        // Act
        var proxima = ProgramaReloj.ProximaDiaria(ahora, TresDeLaManana);

        // Assert  — 03:00 Lima del 21 = 08:00Z del 21
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero), proxima);
    }

    /// <summary>Exactamente a las 03:00 Lima el próximo disparo es a las 03:00 del día siguiente.</summary>
    [Fact]
    public void Exactamente_a_las_tres_va_al_dia_siguiente()
    {
        // Arrange  — exactamente 03:00 Lima = 08:00Z
        var ahora = new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero);

        // Act
        var proxima = ProgramaReloj.ProximaDiaria(ahora, TresDeLaManana);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero), proxima);
    }
}
