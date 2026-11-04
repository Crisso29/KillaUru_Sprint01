using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Tests.Domain;

/// <summary>
/// SC-005 a SC-009 del quickstart: fórmulas por indicador, día nulo en serie, ventanas por suma.
/// </summary>
public sealed class CalculadoraIndicadoresTests
{
    private static readonly DateOnly Base = new(2026, 10, 21);

    private static DiaIndicadores Dia(int idx,
        long ev = 300, long ca = 40, long dd = 15, int t7 = 237, int tt = 336,
        long conf = 18, long pend = 6, long exp = 3, long renac = 2) =>
        new(Base.AddDays(idx), ev, ca, dd, t7, tt, conf, pend, exp, renac);

    // SC-005: Eventos con 7 días de comparación conocidos → comparación y variación exactas
    [Fact]
    public void SC_005_eventos_comparacion_y_porcentaje_relativo()
    {
        // Arrange
        var dias = new DiaIndicadores?[]
        {
            Dia(0, ev: 250), Dia(1, ev: 270), Dia(2, ev: 260),
            Dia(3, ev: 290), Dia(4, ev: 276), Dia(5, ev: 302),
            Dia(6, ev: 300), Dia(7, ev: 321),
        };

        // Act
        var r = CalculadoraIndicadores.CalcularEventos(dias);

        // Assert — avg(250+270+260+290+276+302+300)/7 = 278.3; variación = (321-278.3)/278.3*100 = 15.3
        Assert.Equal(321m,   r.Valor);
        Assert.Equal(278.3m, r.Comparacion);
        Assert.Equal(15.3m,  r.Variacion);
        Assert.Equal(321m,   r.Serie[^1]);
        Assert.Equal(7,      r.Serie.Count);
    }

    // SC-006: Confirmación: 18 conf + 6 pend durante 7 días → 75 %
    [Fact]
    public void SC_006_confirmacion_75_pct()
    {
        // Arrange — 7 días (D-6..D): 3 días conf=4,pend=1 + 3 días conf=2,pend=1 + 1 día conf=0,pend=0
        var dias = new DiaIndicadores?[]
        {
            Dia(0, conf: 4, pend: 1), Dia(1, conf: 3, pend: 1), Dia(2, conf: 3, pend: 1),
            Dia(3, conf: 3, pend: 1), Dia(4, conf: 3, pend: 1), Dia(5, conf: 2, pend: 1),
            Dia(6, conf: 0, pend: 0),
        };

        // Act
        var r = CalculadoraIndicadores.CalcularConfirmacion(dias);

        // Assert — sum conf = 18, sum pend = 6; 18/(18+6)*100 = 75.0
        Assert.Equal(75.0m, r.Valor);
    }

    // SC-007: Reaparición sin expiraciones → valor null
    [Fact]
    public void SC_007_reaparicion_sin_expiraciones_es_null()
    {
        // Arrange — 7 días con exp=0
        var dias = Enumerable.Range(0, 7)
            .Select(i => (DiaIndicadores?)Dia(i, exp: 0, renac: 2))
            .ToList();

        // Act
        var r = CalculadoraIndicadores.CalcularReaparicion(dias);

        // Assert
        Assert.Null(r.Valor);
    }

    // SC-008: Cobertura 237 de 336 → 70.5 %
    [Fact]
    public void SC_008_cobertura_237_de_336()
    {
        // Arrange
        var dias = new DiaIndicadores?[] { Dia(0, t7: 237, tt: 336) };

        // Act
        var r = CalculadoraIndicadores.CalcularCobertura(dias);

        // Assert — 237/336*100 = 70.5357... → 70.5
        Assert.Equal(70.5m, r.Valor);
    }

    // SC-009: Día faltante dentro de la serie → null en su posición; promedios lo ignoran
    [Fact]
    public void SC_009_dia_faltante_en_serie_es_null_y_promedio_lo_ignora()
    {
        // Arrange — 8 días, índice 2 (D-5) es null
        var dias = new DiaIndicadores?[]
        {
            Dia(0, ev: 100), Dia(1, ev: 100), null,
            Dia(3, ev: 100), Dia(4, ev: 100), Dia(5, ev: 100),
            Dia(6, ev: 100), Dia(7, ev: 200),
        };

        // Act
        var r = CalculadoraIndicadores.CalcularEventos(dias);

        // Assert — serie[1] = D-5 = null (el null está en la posición 2 del array que son 8 elementos:
        // serie = [dias[1], null, dias[3], dias[4], dias[5], dias[6], dias[7]] )
        // Para 8 elementos: serie7 cubre indices 1..7 (fromEnd 7..1)
        // k=0 → fe=7 → dias[^7]=dias[1]=100
        // k=1 → fe=6 → dias[^6]=dias[2]=null
        Assert.Null(r.Serie[1]);
        // Comparación ignora el nulo (D-7..D-1 = indices 0..6; null en idx=2 se salta)
        // 6 valores válidos de 100: avg = 100
        Assert.Equal(100m, r.Comparacion);
    }

    [Fact]
    public void Conductores_variacion_es_diferencia_absoluta()
    {
        // Arrange — 8 días, D=42, D-7..D-1 avg=40
        var dias = new DiaIndicadores?[]
        {
            Dia(0, ca: 40), Dia(1, ca: 40), Dia(2, ca: 40), Dia(3, ca: 40),
            Dia(4, ca: 40), Dia(5, ca: 40), Dia(6, ca: 40), Dia(7, ca: 42),
        };

        // Act
        var r = CalculadoraIndicadores.CalcularConductores(dias);

        // Assert — variación = 42 - 40 = 2 (diferencia absoluta, no porcentual)
        Assert.Equal(42m, r.Valor);
        Assert.Equal(40m, r.Comparacion);
        Assert.Equal(2m,  r.Variacion);
    }

    [Fact]
    public void Sin_datos_devuelve_todo_null()
    {
        // Arrange
        var vacio = Array.Empty<DiaIndicadores?>();

        // Act & Assert
        Assert.Null(CalculadoraIndicadores.CalcularEventos(vacio).Valor);
        Assert.Null(CalculadoraIndicadores.CalcularConfirmacion(vacio).Valor);
        Assert.Null(CalculadoraIndicadores.CalcularCobertura(vacio).Valor);
    }
}
