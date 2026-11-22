using System.Text;
using KillaUru.Ingesta.Adapters;

namespace KillaUru.Ingesta.Tests.Adapters;

/// <summary>
/// El cuerpo del lote se lee con un tope de bytes tomado de `Ingesta:TamanoMaximoCuerpoKb`: más de ese tope es 413
/// `lote_demasiado_grande`, y un cuerpo que no es `{ "eventos": [...] }` es 400 `lote_invalido` (contrato 005).
/// Spec 005 · T012 · FR-001 (R-002).
/// </summary>
public sealed class LectorCuerpoLoteTests
{
    private const long Tope = 64;

    private static Task<LecturaLote> LeerAsync(string cuerpo, long tope = Tope) =>
        LectorCuerpoLote.LeerAsync(new MemoryStream(Encoding.UTF8.GetBytes(cuerpo)), null, tope, TestContext.Current.CancellationToken);

    /// <summary>Si `Content-Length` ya declara más que el tope, es 413 sin leer el cuerpo (spec 005, casos borde).</summary>
    [Fact]
    public async Task Content_Length_mayor_que_el_tope_es_demasiado_grande_sin_leer()
    {
        // Arrange — el doble lanza si alguien intenta leerlo
        using var cuerpo = new CuerpoCortadoPorElServidor();

        // Act
        var lectura = await LectorCuerpoLote.LeerAsync(cuerpo, Tope + 1, Tope, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(EstadoLectura.DemasiadoGrande, lectura.Estado);
        Assert.False(cuerpo.FueLeido);
    }

    /// <summary>Un lote bien formado se entrega con sus eventos, sin interpretar todavía cada evento.</summary>
    [Fact]
    public async Task Lote_con_arreglo_eventos_se_lee()
    {
        // Arrange
        const string cuerpo = """{ "eventos": [ {}, { "x": 1 } ] }""";

        // Act
        var lectura = await LeerAsync(cuerpo);

        // Assert
        Assert.Equal(EstadoLectura.Leido, lectura.Estado);
        Assert.Equal(2, lectura.Eventos!.Count);
    }

    /// <summary>El tope es inclusivo: «más de N bytes» es 413; exactamente N bytes todavía se lee.</summary>
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public async Task Tope_de_bytes_es_inclusivo(int bytesDeMas, bool cabe)
    {
        // Arrange — rellena con espacios (JSON válido) hasta el tope exacto más los bytes indicados
        const string lote = """{"eventos":[]}""";
        var cuerpo = lote + new string(' ', (int)Tope - lote.Length + bytesDeMas);

        // Act
        var lectura = await LeerAsync(cuerpo);

        // Assert
        Assert.Equal(cabe ? EstadoLectura.Leido : EstadoLectura.DemasiadoGrande, lectura.Estado);
    }

    /// <summary>JSON roto, vacío, sin `eventos`, con `eventos` nulo o que no es arreglo: 400 `lote_invalido`.</summary>
    [Theory]
    [InlineData("esto no es JSON")]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("""{ "eventos": null }""")]
    [InlineData("""{ "eventos": 5 }""")]
    [InlineData("[]")]
    public async Task Cuerpo_que_no_es_un_lote_es_invalido(string cuerpo)
    {
        // Arrange — el cuerpo llega tal cual por la red

        // Act
        var lectura = await LeerAsync(cuerpo);

        // Assert
        Assert.Equal(EstadoLectura.Invalido, lectura.Estado);
        Assert.Null(lectura.Eventos);
    }

    /// <summary>Si Kestrel corta la lectura por su propio límite (413), también es `lote_demasiado_grande`, no un 500.</summary>
    [Fact]
    public async Task Corte_del_servidor_por_tamano_es_demasiado_grande()
    {
        // Arrange
        using var cuerpo = new CuerpoCortadoPorElServidor();

        // Act
        var lectura = await LectorCuerpoLote.LeerAsync(cuerpo, null, Tope, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(EstadoLectura.DemasiadoGrande, lectura.Estado);
        Assert.True(cuerpo.FueLeido);
    }
}
