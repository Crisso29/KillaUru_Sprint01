using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// Vencimiento del token de actualización: el conductor desliza 30 días desde el último uso;
/// el administrador renueva de 8 en 8 h sin pasar su límite absoluto. Spec 003 · FR-015, FR-039.
/// </summary>
public sealed class PoliticaSesionTests
{
    private const int DiasConductor = 30;
    private const int HorasAdministrador = 8;

    /// <summary>Sin límite absoluto (conductor), vence 30 días después, truncado a la hora.</summary>
    [Fact]
    public void Conductor_desliza_treinta_dias()
    {
        // Act
        var vence = PoliticaSesion.Vencimiento(Instantes.Ahora, limiteAbsoluto: null, DiasConductor, HorasAdministrador);

        // Assert
        Assert.Equal(HorasGuardadas.ALaHora(Instantes.Ahora.AddDays(30)), vence);
    }

    /// <summary>Si el límite absoluto llega antes que las 8 h, manda el límite.</summary>
    [Fact]
    public void Administrador_no_pasa_su_limite_absoluto()
    {
        // Arrange
        var limite = Instantes.Ahora.AddHours(3);

        // Act
        var vence = PoliticaSesion.Vencimiento(Instantes.Ahora, limite, DiasConductor, HorasAdministrador);

        // Assert
        Assert.Equal(HorasGuardadas.ALaHora(limite), vence);
    }

    /// <summary>Si el límite absoluto está lejos, el administrador renueva solo 8 h.</summary>
    [Fact]
    public void Administrador_renueva_ocho_horas()
    {
        // Arrange
        var limite = Instantes.Ahora.AddHours(10);

        // Act
        var vence = PoliticaSesion.Vencimiento(Instantes.Ahora, limite, DiasConductor, HorasAdministrador);

        // Assert
        Assert.Equal(HorasGuardadas.ALaHora(Instantes.Ahora.AddHours(8)), vence);
    }
}
