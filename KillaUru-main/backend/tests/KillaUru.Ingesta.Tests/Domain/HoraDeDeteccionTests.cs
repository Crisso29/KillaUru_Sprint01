using KillaUru.Ingesta.Domain;

namespace KillaUru.Ingesta.Tests.Domain;

/// <summary>
/// HoraDeDeteccion mapea cualquier instante a su bloque horario UTC [hh:00, hh:00+1h).
/// Se usa para contar los eventos del mismo seudónimo en la hora del detector (RN-15a).
/// Spec 005 · T006.
/// </summary>
public sealed class HoraDeDeteccionTests
{
    /// <summary>Un instante exactamente en el límite inferior pertenece al mismo bloque (incluido).</summary>
    [Fact]
    public void Limite_inferior_pertenece_al_bloque()
    {
        // Arrange
        var t = new DateTimeOffset(2026, 10, 20, 14, 0, 0, TimeSpan.Zero);

        // Act
        var (inicio, fin) = HoraDeDeteccion.Bloque(t);

        // Assert
        Assert.Equal(t, inicio);
        Assert.Equal(t.AddHours(1), fin);
    }

    /// <summary>14:37:12Z pertenece al bloque [14:00Z, 15:00Z).</summary>
    [Fact]
    public void Instante_intermedio_cae_en_su_bloque_de_hora()
    {
        // Arrange
        var t = new DateTimeOffset(2026, 10, 20, 14, 37, 12, TimeSpan.Zero);

        // Act
        var (inicio, fin) = HoraDeDeteccion.Bloque(t);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 14, 0, 0, TimeSpan.Zero), inicio);
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero), fin);
    }

    /// <summary>22:59:59 Lima (UTC-5) = 03:59:59Z cae en el bloque UTC [03:00Z, 04:00Z).</summary>
    [Fact]
    public void Hora_local_lima_se_convierte_a_utc_antes_de_calcular_el_bloque()
    {
        // Arrange  — 22:59:59 Lima = 2026-10-21 03:59:59Z
        var t = new DateTimeOffset(2026, 10, 20, 22, 59, 59, TimeSpan.FromHours(-5));

        // Act
        var (inicio, _) = HoraDeDeteccion.Bloque(t);

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 3, 0, 0, TimeSpan.Zero), inicio);
    }

    /// <summary>El límite superior (hh:00:00 exacto) ya pertenece al siguiente bloque (excluido).</summary>
    [Fact]
    public void Limite_superior_no_pertenece_al_bloque()
    {
        // Arrange
        var tFin = new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero);

        // Act
        var (inicio, _) = HoraDeDeteccion.Bloque(tFin);

        // Assert — 15:00 pertenece al bloque [15:00, 16:00), no al anterior
        Assert.Equal(tFin, inicio);
    }
}
