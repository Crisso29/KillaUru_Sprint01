using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>Comprueba plazos persistentes y la unicidad del historial al confirmar o renacer.</summary>
public sealed class CicloVidaTests
{
    [Fact]
    public void Persistente_activa_vence_en_siete_dias()
    {
        var r = ReportesPrueba.Crear();
        var a = Anomalia.Crear(r, false, ReportesPrueba.Ahora);
        a.RegistrarReporte(r, [r, r with { DetectadoUtc = r.DetectadoUtc.AddDays(-1) },
            r with { DetectadoUtc = r.DetectadoUtc.AddDays(-2) }], new(), ReportesPrueba.Ahora);
        Assert.Equal(EstadoPersistencia.Persistente, a.Persistencia);
        Assert.Equal(r.DetectadoUtc.AddDays(7), a.VenceUtc);
        Assert.Single(a.Transiciones);
    }

    [Fact]
    public void Confirmacion_agrega_una_sola_transicion()
    {
        var r = ReportesPrueba.Crear(5, sesion: true);
        var a = Anomalia.Crear(r, false, ReportesPrueba.Ahora);
        a.RegistrarReporte(r, [r], new(), ReportesPrueba.Ahora);
        var segundo = ReportesPrueba.Crear(5, 180);
        a.RegistrarReporte(segundo, [r, segundo], new(), segundo.DetectadoUtc);
        Assert.Equal(2, a.Transiciones.Count);
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, a.Transiciones[1].Desde);
        Assert.Equal(EstadoAnomalia.Activa, a.Transiciones[1].Hacia);
    }

    [Fact]
    public void Leve_expirada_renace_directamente_activa()
    {
        var r = ReportesPrueba.Crear();
        var a = Anomalia.Crear(r, false, ReportesPrueba.Ahora);
        a.RegistrarReporte(r, [r], new(), ReportesPrueba.Ahora);
        a.Caducar(a.VenceUtc);
        var nuevo = r with { EventoId = Guid.NewGuid(), DetectadoUtc = a.VenceUtc.AddSeconds(1) };
        a.RegistrarReporte(nuevo, [nuevo], new(), nuevo.DetectadoUtc);
        Assert.Equal(3, a.Transiciones.Count);
        Assert.Equal(EstadoAnomalia.Expirada, a.Transiciones[2].Desde);
        Assert.Equal(EstadoAnomalia.Activa, a.Transiciones[2].Hacia);
    }
}
