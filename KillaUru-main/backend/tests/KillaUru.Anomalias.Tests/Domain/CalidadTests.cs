using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// RN-11: reglas de recepción por evento, puras y sin base de datos, con límites inclusivos y la
/// antigüedad medida contra la hora de recepción original. Un campo inválido descarta solo ese
/// evento, nunca el lote. Spec 004 · FR-001, FR-002.
/// </summary>
public sealed class CalidadTests
{
    private static MotivoDescarte? Evaluar(ReporteAnomalia reporte) =>
        new ReglasCalidad(new(), ReportesPrueba.Corredor()).EvaluarIntrinseca(reporte, ReportesPrueba.Ahora);

    /// <summary>Confidence mínimo 0,75 inclusive (FR-001).</summary>
    [Theory]
    [InlineData(0.74, (int)MotivoDescarte.ConfidenceBajo)]
    [InlineData(0.75, null)]
    [InlineData(0.76, null)]
    public void Confidence_minimo_es_inclusivo(double confidence, int? motivoEsperado)
    {
        // Arrange
        var reporte = ReportesPrueba.Crear() with { Confidence = Confidence.Crear(confidence)!.Value, PrecisionMetros = 20 };

        // Act
        var motivo = Evaluar(reporte);

        // Assert
        Assert.Equal((MotivoDescarte?)motivoEsperado, motivo);
    }

    /// <summary>Precisión GPS máxima de 20 m inclusive (FR-001).</summary>
    [Theory]
    [InlineData(19, null)]
    [InlineData(20, null)]
    [InlineData(21, (int)MotivoDescarte.PrecisionGpsInsuficiente)]
    public void Precision_gps_maxima_es_inclusiva(double precisionMetros, int? motivoEsperado)
    {
        // Arrange
        var reporte = ReportesPrueba.Crear() with { PrecisionMetros = precisionMetros };

        // Act
        var motivo = Evaluar(reporte);

        // Assert
        Assert.Equal((MotivoDescarte?)motivoEsperado, motivo);
    }

    /// <summary>
    /// Se acepta hasta 7 días de antigüedad y 5 min en el futuro (reloj del celular adelantado),
    /// ambos inclusive y medidos contra la recepción (FR-002).
    /// </summary>
    [Theory]
    [InlineData(-604800, null)]
    [InlineData(-604801, (int)MotivoDescarte.TimestampFueraDeRango)]
    [InlineData(299, null)]
    [InlineData(300, null)]
    [InlineData(301, (int)MotivoDescarte.TimestampFueraDeRango)]
    public void Ventana_de_tiempo_es_inclusiva(int segundosDesdeRecepcion, int? motivoEsperado)
    {
        // Arrange
        var reporte = ReportesPrueba.Crear(segundos: segundosDesdeRecepcion) with { PrecisionMetros = 20 };

        // Act
        var motivo = Evaluar(reporte);

        // Assert
        Assert.Equal((MotivoDescarte?)motivoEsperado, motivo);
    }

    /// <summary>«Normal» o un número no son tipos del catálogo; sin tipo, el esquema es inválido (FR-001).</summary>
    [Theory]
    [InlineData("Normal", (int)MotivoDescarte.TipoDesconocido)]
    [InlineData("3", (int)MotivoDescarte.TipoDesconocido)]
    [InlineData(null, (int)MotivoDescarte.EsquemaInvalido)]
    public void Tipo_invalido_descarta_solo_ese_evento(string? tipo, int motivoEsperado)
    {
        // Arrange
        var entrada = ReportesPrueba.Entrada() with { Tipo = tipo };

        // Act
        var (_, motivo) = ReporteAnomalia.Desde(entrada, ReportesPrueba.Contexto());

        // Assert
        Assert.Equal((MotivoDescarte?)motivoEsperado, motivo);
    }

    /// <summary>Cada caso estropea un solo campo de una entrada válida.</summary>
    private static readonly Dictionary<string, Func<ReporteEntrada, ReporteEntrada>> Estropear = new()
    {
        ["latitud NaN"] = e => e with { Latitud = double.NaN },
        ["precisión infinita"] = e => e with { PrecisionMetros = double.PositiveInfinity },
        ["severidad 6"] = e => e with { Severidad = 6 },
        ["seudónimo vacío"] = e => e with { Seudonimo = Guid.Empty },
        ["eventoId vacío"] = e => e with { EventoId = Guid.Empty },
        ["modelo en blanco"] = e => e with { ModeloVersion = " " },
    };

    /// <summary>Valores ausentes, no finitos o fuera de rango son EsquemaInvalido, sin excepción (FR-001).</summary>
    [Theory]
    [InlineData("latitud NaN")]
    [InlineData("precisión infinita")]
    [InlineData("severidad 6")]
    [InlineData("seudónimo vacío")]
    [InlineData("eventoId vacío")]
    [InlineData("modelo en blanco")]
    public void Valor_ausente_o_no_finito_es_esquema_invalido(string caso)
    {
        // Arrange
        var entrada = Estropear[caso](ReportesPrueba.Entrada());

        // Act
        var (reporte, motivo) = ReporteAnomalia.Desde(entrada, ReportesPrueba.Contexto());

        // Assert
        Assert.Null(reporte);
        Assert.Equal(MotivoDescarte.EsquemaInvalido, motivo);
    }

    /// <summary>Una entrada completa produce el reporte validado.</summary>
    [Fact]
    public void Entrada_valida_produce_el_reporte()
    {
        // Act
        var (reporte, motivo) = ReporteAnomalia.Desde(ReportesPrueba.Entrada(), ReportesPrueba.Contexto(conSesion: true));

        // Assert
        Assert.NotNull(reporte);
        Assert.Null(motivo);
    }
}
