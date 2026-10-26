using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>Valores inclusivos de RN-11, evaluados con la recepción original.</summary>
public sealed class CalidadTests
{
    [Theory]
    [InlineData(0.74, 20, 0, (int)MotivoDescarte.ConfidenceBajo)]
    [InlineData(0.75, 20, 0, null)]
    [InlineData(0.76, 19, 0, null)]
    [InlineData(0.9, 21, 0, (int)MotivoDescarte.PrecisionGpsInsuficiente)]
    [InlineData(0.9, 20, -604800, null)]
    [InlineData(0.9, 20, -604801, (int)MotivoDescarte.TimestampFueraDeRango)]
    [InlineData(0.9, 20, 299, null)]
    [InlineData(0.9, 20, 300, null)]
    [InlineData(0.9, 20, 301, (int)MotivoDescarte.TimestampFueraDeRango)]
    public void Limites(double confidence, double precision, int segundos, int? motivo)
    {
        var r = ReportesPrueba.Crear(segundos: segundos) with { Confidence = Confidence.Crear(confidence)!.Value, PrecisionMetros = precision };
        Assert.Equal((MotivoDescarte?)motivo, new ReglasCalidad(new(), ReportesPrueba.Corredor()).EvaluarIntrinseca(r, ReportesPrueba.Ahora));
    }
    [Theory]
    [InlineData("Normal", (int)MotivoDescarte.TipoDesconocido)]
    [InlineData("3", (int)MotivoDescarte.TipoDesconocido)]
    [InlineData(null, (int)MotivoDescarte.EsquemaInvalido)]
    public void Tipo_invalido_no_rompe_el_lote(string? tipo, int motivo) =>
        Assert.Equal((MotivoDescarte?)motivo, ReporteAnomalia.Desde(ReportesPrueba.Entrada() with { Tipo = tipo }, new(false, 1, ReportesPrueba.Ahora)).Motivo);
    [Fact]
    public void Valores_no_finitos_y_ausentes_se_rechazan()
    {
        var e = ReportesPrueba.Entrada();
        foreach (var invalido in new[] { e with { Latitud = double.NaN }, e with { PrecisionMetros = double.PositiveInfinity },
            e with { Severidad = 6 }, e with { Seudonimo = Guid.Empty }, e with { EventoId = Guid.Empty }, e with { ModeloVersion = " " } })
        {
            Assert.Equal(MotivoDescarte.EsquemaInvalido, ReporteAnomalia.Desde(invalido, new(false, 1, ReportesPrueba.Ahora)).Motivo);
        }
        Assert.NotNull(ReporteAnomalia.Desde(e, new(true, 1, ReportesPrueba.Ahora)).Reporte);
    }
}
