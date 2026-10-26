using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>El agregado conserva posición, cronología y transiciones incluso con reportes tardíos.</summary>
public sealed class AnomaliaTests
{
    [Theory]
    [InlineData(2, false, (int)EstadoAnomalia.Activa)]
    [InlineData(5, false, (int)EstadoAnomalia.PendienteConfirmacion)]
    [InlineData(2, true, (int)EstadoAnomalia.PendienteConfirmacion)]
    public void Nacimiento(int severidad, bool tope, int esperado)
    {
        var r = ReportesPrueba.Crear(severidad) with { KmVia = 22.5f };
        var a = Anomalia.Crear(r, tope, ReportesPrueba.Ahora);
        a.RegistrarReporte(r, [r], new(), ReportesPrueba.Ahora);
        Assert.Equal((EstadoAnomalia)esperado, a.Estado); Assert.Equal(22.5f, a.KmVia);
        Assert.Equal(r.DetectadoUtc.AddHours(72), a.VenceUtc);
        Assert.Single(a.Transiciones); Assert.Null(a.Transiciones[0].Desde);
    }

    [Fact]
    public void Activa_no_regresa_a_pendiente_y_ultimo_no_retrocede()
    {
        var r = ReportesPrueba.Crear(3); var a = Anomalia.Crear(r, false, ReportesPrueba.Ahora);
        a.RegistrarReporte(r, [r], new(), ReportesPrueba.Ahora);
        var viejo = ReportesPrueba.Crear(4, -60) with { KmVia = 99 };
        a.RegistrarReporte(viejo, [r, viejo], new(), ReportesPrueba.Ahora);
        Assert.Equal(EstadoAnomalia.Activa, a.Estado); Assert.Equal(4, a.SeveridadMostrada.Valor);
        Assert.Equal(r.DetectadoUtc, a.UltimaDeteccionUtc); Assert.Equal(0, a.KmVia);
        Assert.Single(a.Transiciones);
    }

    [Fact]
    public void Expirada_y_reparada_renacen_solo_despues_del_limite()
    {
        var r = ReportesPrueba.Crear(); var o = new AnomaliasOptions();
        var a = Anomalia.Crear(r, false, ReportesPrueba.Ahora); a.RegistrarReporte(r, [r], o, ReportesPrueba.Ahora);
        var vence = a.VenceUtc; a.Caducar(vence);
        Assert.Equal(EstadoAnomalia.Expirada, a.Estado);
        a.RegistrarReporte(r, [r], o, vence); Assert.Equal(EstadoAnomalia.Expirada, a.Estado);
        var nuevo = ReportesPrueba.Crear(5) with { DetectadoUtc = vence.AddSeconds(1) };
        a.RegistrarReporte(nuevo, [nuevo], o, vence.AddSeconds(1));
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, a.Estado);
        Assert.Equal(3, a.Transiciones.Count);
        Assert.True(a.Moderar(AccionAdmin.MarcarReparada, vence.AddSeconds(2), o, Guid.NewGuid()));
        nuevo = nuevo with { DetectadoUtc = vence.AddSeconds(3) };
        a.RegistrarReporte(nuevo, [nuevo], o, vence.AddSeconds(3));
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, a.Estado);
        Assert.Equal(5, a.Transiciones.Count);
    }

    [Fact]
    public void Descartada_absorbe_y_mantener_exige_revision()
    {
        var r = ReportesPrueba.Crear(); var o = new AnomaliasOptions();
        var a = Anomalia.Crear(r, false, ReportesPrueba.Ahora); a.RegistrarReporte(r, [r], o, ReportesPrueba.Ahora);
        Assert.True(a.Moderar(AccionAdmin.Descartar, ReportesPrueba.Ahora, o, Guid.NewGuid()));
        Assert.False(a.Moderar(AccionAdmin.MantenerDescartada, ReportesPrueba.Ahora, o, Guid.NewGuid()));
        var ventana = new List<ReporteAnomalia> { r };
        for (var i = 1; i <= 3; i++)
        {
            var otro = ReportesPrueba.Crear(segundos: i * 180); ventana.Add(otro);
            a.RegistrarReporte(otro, ventana, o, otro.DetectadoUtc);
        }
        Assert.True(a.PorRevisar); Assert.Equal(EstadoAnomalia.Descartada, a.Estado);
        Assert.Equal(ReportesPrueba.Ahora.AddMinutes(9), a.UltimaDeteccionUtc);
        Assert.True(a.Moderar(AccionAdmin.MantenerDescartada, ReportesPrueba.Ahora.AddMinutes(10), o, Guid.NewGuid()));
        Assert.False(a.PorRevisar); Assert.Equal(2, a.Transiciones.Count);
    }
}
