using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>Matriz completa RN-07, incluida la condición adicional de mantener descartada.</summary>
public sealed class ModeracionTablaTests
{
    public static IEnumerable<object[]> Casos()
    {
        int?[,] esperado = { {4,5,null,null}, {4,5,null,null}, {null,5,null,null}, {null,null,1,null}, {null,null,1,5} };
        for (var estado = 1; estado <= 5; estado++)
            for (var accion = 0; accion < 4; accion++) yield return new object[] { estado, accion, esperado[estado-1, accion]! };
    }
    [Theory, MemberData(nameof(Casos))]
    public void Matriz_de_veinte_combinaciones(int estado, int accion, int? esperado) =>
        Assert.Equal((EstadoAnomalia?)esperado, TablaModeracion.Destino((EstadoAnomalia)estado, (AccionAdmin)accion, true));
    [Fact]
    public void Mantener_descartada_requiere_revision() =>
        Assert.Null(TablaModeracion.Destino(EstadoAnomalia.Descartada, AccionAdmin.MantenerDescartada, false));
    [Fact]
    public void Tres_vehiculos_posteriores_marcan_revision_y_mantener_limpia_sin_transicion()
    {
        var inicial = ReportesPrueba.Crear(); var ventana = new List<ReporteAnomalia> { inicial };
        var a = Anomalia.Crear(inicial, false, ReportesPrueba.Ahora); a.RegistrarReporte(inicial, ventana, new(), ReportesPrueba.Ahora);
        Assert.True(a.Moderar(AccionAdmin.Descartar, ReportesPrueba.Ahora, new(), Guid.NewGuid()));
        for (var i=1; i<=3; i++)
        {
            var nuevo = ReportesPrueba.Crear(segundos: i*180); ventana.Add(nuevo);
            a.RegistrarReporte(nuevo, ventana, new(), nuevo.DetectadoUtc);
        }
        Assert.True(a.PorRevisar); Assert.Equal(3, a.MarcasRevision.Count);
        var cantidad = a.Transiciones.Count;
        Assert.True(a.Moderar(AccionAdmin.MantenerDescartada, ReportesPrueba.Ahora.AddHours(1), new(), Guid.NewGuid()));
        Assert.False(a.PorRevisar); Assert.Empty(a.MarcasRevision); Assert.Equal(cantidad, a.Transiciones.Count);
    }
}
