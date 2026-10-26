using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// RN-19: tras 3 fallos en 15 min la cuenta se bloquea 5, 15 y 60 min (creciente); un inicio
/// correcto reinicia contador y nivel. Spec 003 · FR-009.
/// </summary>
public sealed class UsuarioBloqueoTests
{
    private static readonly ParametrosBloqueo Parametros = new(IntentosMaximos: 3, VentanaMinutos: 15, DuracionesMinutos: [5, 15, 60]);

    private static void Fallar(PoliticaBloqueo politica, int veces, DateTimeOffset instante)
    {
        for (var intento = 0; intento < veces; intento++) politica.RegistrarFallo(instante, Parametros);
    }

    /// <summary>Los dos primeros fallos no bloquean; el tercero bloquea 5 min (fin exclusivo).</summary>
    [Fact]
    public void Tercer_fallo_bloquea_cinco_minutos()
    {
        // Arrange
        var politica = new PoliticaBloqueo();
        Fallar(politica, 2, Instantes.Ahora);

        // Act
        var bloqueo = politica.RegistrarFallo(Instantes.Ahora, Parametros);

        // Assert
        Assert.True(bloqueo);
        Assert.Equal(Instantes.Ahora.AddMinutes(5), politica.BloqueadoHasta);
        Assert.True(politica.EstaBloqueado(Instantes.Ahora.AddMinutes(4)));
        Assert.False(politica.EstaBloqueado(Instantes.Ahora.AddMinutes(5)));
    }

    /// <summary>Un inicio correcto borra el historial: el siguiente bloqueo vuelve a ser de 5 min.</summary>
    [Fact]
    public void Exito_reinicia_contador_y_nivel()
    {
        // Arrange
        var politica = new PoliticaBloqueo();
        Fallar(politica, 3, Instantes.Ahora);

        // Act
        politica.RegistrarExito();

        // Assert
        Assert.Equal(0, politica.NivelBloqueo);
        Assert.Equal(0, politica.IntentosFallidos);
        Assert.Null(politica.BloqueadoHasta);
    }

    /// <summary>La ventana de 15 min es inclusiva: el fallo en el segundo 900 todavía cuenta.</summary>
    [Theory]
    [InlineData(900, true)]
    [InlineData(901, false)]
    public void Ventana_de_fallos_es_inclusiva(int segundosDesdePrimerFallo, bool bloquea)
    {
        // Arrange
        var politica = new PoliticaBloqueo();
        Fallar(politica, 2, Instantes.Ahora);

        // Act
        var bloqueo = politica.RegistrarFallo(Instantes.Ahora.AddSeconds(segundosDesdePrimerFallo), Parametros);

        // Assert
        Assert.Equal(bloquea, bloqueo);
    }

    /// <summary>Las duraciones crecen 5 → 15 → 60 y se quedan en el último nivel.</summary>
    [Fact]
    public void Bloqueos_crecen_hasta_el_ultimo_nivel()
    {
        // Arrange
        var politica = new PoliticaBloqueo();
        var instante = Instantes.Ahora;
        var duracionesObtenidas = new List<double>();

        // Act
        for (var bloqueo = 0; bloqueo < 4; bloqueo++)
        {
            Fallar(politica, 3, instante);
            duracionesObtenidas.Add((politica.BloqueadoHasta!.Value - instante).TotalMinutes);
            instante = politica.BloqueadoHasta.Value;
        }

        // Assert
        Assert.Equal([5, 15, 60, 60], duracionesObtenidas);
    }

    /// <summary>
    /// Mientras dura el bloqueo, los fallos no cuentan: el atacante no puede alargarlo ni
    /// adelantar el siguiente nivel.
    /// </summary>
    [Fact]
    public void Fallo_durante_el_bloqueo_no_cuenta()
    {
        // Arrange
        var politica = new PoliticaBloqueo();
        var unSoloIntento = new ParametrosBloqueo(1, 15, [5]);
        politica.RegistrarFallo(Instantes.Ahora, unSoloIntento);

        // Act
        var bloqueoNuevo = politica.RegistrarFallo(Instantes.Ahora, unSoloIntento);

        // Assert
        Assert.False(bloqueoNuevo);
        Assert.Equal(0, politica.IntentosFallidos);
        Assert.Null(politica.VentanaFallosDesdeUtc);
    }
}
