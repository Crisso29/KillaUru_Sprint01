using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>Confirmación, topes y persistencia con límites del catálogo de negocio.</summary>
public sealed class PoliticasTests
{
    [Theory]
    [InlineData(179, true, false, 4, (int)EstadoAnomalia.PendienteConfirmacion)]
    [InlineData(180, true, false, 4, (int)EstadoAnomalia.Activa)]
    [InlineData(180, false, false, 5, (int)EstadoAnomalia.PendienteConfirmacion)]
    [InlineData(180, false, true, 2, (int)EstadoAnomalia.Activa)]
    [InlineData(0, true, false, 4, (int)EstadoAnomalia.PendienteConfirmacion)]
    public void Confirmacion(int segundos, bool sesion, bool tope, int severidad, int esperado)
    {
        var a = ReportesPrueba.Crear(severidad, sesion: sesion);
        var b = ReportesPrueba.Crear(2, segundos);
        Assert.Equal((EstadoAnomalia)esperado, new PoliticaConfirmacion(new()).Decidir([a, b], a.Severidad, tope, b.DetectadoUtc));
    }
    [Fact]
    public void Mismo_seudonimo_o_purgado_no_confirma()
    {
        var a = ReportesPrueba.Crear(5, sesion: true);
        var b = ReportesPrueba.Crear(5, 180) with { Seudonimo = a.Seudonimo };
        var p = new PoliticaConfirmacion(new());
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, p.Decidir([a, b], a.Severidad, false, b.DetectadoUtc));
        Assert.False(p.EsVehiculoNuevo(b, [a, b]));
        Assert.False(p.EsVehiculoNuevo(b with { Seudonimo = null }, []));
        Assert.True(p.EsVehiculoNuevo(b with { Seudonimo = Guid.NewGuid() }, [a]));
    }
    [Theory]
    [InlineData(9, 160, null)]
    [InlineData(10, 150, null)]
    [InlineData(10, 151, (int)MotivoDescarte.VelocidadImposible)]
    public void Velocidad(int segundos, double kmh, int? esperado)
    {
        var anterior = ReportesPrueba.Crear();
        var grados = kmh / 3.6 * segundos / 6371008.8 * 180 / Math.PI;
        var actual = anterior with { EventoId = Guid.NewGuid(), DetectadoUtc = anterior.DetectadoUtc.AddSeconds(segundos), Posicion = ReportesPrueba.Punto(0, grados) };
        var p = new PoliticaPlausibilidad(new());
        Assert.Equal((MotivoDescarte?)esperado, p.Evaluar(actual, anterior));
        Assert.Null(p.Evaluar(actual with { PrecisionMetros = 20 }, anterior with { PrecisionMetros = 20 }));
        Assert.Null(p.Evaluar(actual, null));
        Assert.False(p.SuperaTope(120)); Assert.True(p.SuperaTope(121));
    }
    [Fact]
    public void Dias_locales_y_ventana_no_cuentan_futuro()
    {
        var fin = new DateTimeOffset(2026, 10, 24, 5, 0, 0, TimeSpan.Zero);
        var regla = new ReglaPersistencia(new());
        Assert.Equal(EstadoPersistencia.Episodica, regla.Evaluar([fin, fin.AddHours(-1)], fin));
        Assert.Equal(EstadoPersistencia.Persistente, regla.Evaluar([fin, fin.AddHours(-1), fin.AddHours(-26)], fin));
        Assert.Equal(EstadoPersistencia.Episodica, regla.Evaluar([fin, fin.AddDays(-16), fin.AddDays(1)], fin));
    }
}
