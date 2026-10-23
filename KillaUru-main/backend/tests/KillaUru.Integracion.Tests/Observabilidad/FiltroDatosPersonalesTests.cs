using KillaUru.Api.Observabilidad;
using Serilog.Events;
using Serilog.Parsing;

namespace KillaUru.Integracion.Tests.Observabilidad;

/// <summary>Parte de T067: eliminación de propiedades sensibles de primer nivel en eventos Serilog sintéticos.</summary>
/// <remarks>No captura peticiones, trazas, excepciones ni estructuras anidadas; no acredita todo FR-034.</remarks>
public sealed class FiltroDatosPersonalesTests
{
    private static LogEvent Evento(params (string Nombre, object Valor)[] propiedades) => new(
        DateTimeOffset.UnixEpoch, LogEventLevel.Information, null, new MessageTemplateParser().Parse("x"),
        propiedades.Select(p => new LogEventProperty(p.Nombre, new ScalarValue(p.Valor))));

    [Theory]
    [InlineData("Seudonimo")]
    [InlineData("RemoteIpAddress")]
    [InlineData("Authorization")]
    [InlineData("Cookie")]
    [InlineData("AccessToken")]
    [InlineData("Correo")]
    [InlineData("Email")]
    public void Quita_las_propiedades_sensibles(string nombre)
    {
        var evento = Evento((nombre, "dato"), ("Ruta", "/api/v1/prueba"));

        new FiltroDatosPersonales().Enrich(evento, null!);

        Assert.False(evento.Properties.ContainsKey(nombre));
        Assert.True(evento.Properties.ContainsKey("Ruta"));
    }

    [Fact]
    public void Nunca_deja_juntos_el_sujeto_y_el_loteId()
    {
        var evento = Evento(("Sub", "cuenta-1"), ("LoteId", Guid.NewGuid()));

        new FiltroDatosPersonales().Enrich(evento, null!);

        Assert.False(evento.Properties.ContainsKey("Sub"));
        Assert.True(evento.Properties.ContainsKey("LoteId"));
    }

    [Fact]
    public void Sin_loteId_el_sujeto_se_conserva_para_auditar_accesos()
    {
        var evento = Evento(("Sub", "cuenta-1"));

        new FiltroDatosPersonales().Enrich(evento, null!);

        Assert.True(evento.Properties.ContainsKey("Sub"));
    }
}
