using System.Net;
using System.Text;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Lotes que no deben guardarse: inválidos (400), demasiado grandes (413) o con la cola saturada (503).
/// Los errores siguen el contrato: `application/problem+json` con su `codigo`. Escenarios #7, #8 y #12 del
/// quickstart de 005. Spec 005 · T012 · FR-001, FR-007, FR-023 (contrato `ingesta.openapi.yaml`).
/// </summary>
public sealed class RecepcionLimitesApiTests(BaseDatosPrueba bd)
{
    private static StringContent Texto(string cuerpo) => new(cuerpo, Encoding.UTF8, "application/json");

    public static TheoryData<string> CuerposInvalidos() => new()
    {
        { "vacio" }, { "51 eventos" }, { "sin arreglo eventos" }, { "no es JSON" },
    };

    private static HttpContent Cuerpo(LoteDePrueba lote, string caso) => caso switch
    {
        "vacio" => LoteDePrueba.Cuerpo([]),
        "51 eventos" => LoteDePrueba.Cuerpo(lote.Eventos(51)),
        "sin arreglo eventos" => Texto("""{ "otro": [] }"""),
        _ => Texto("esto no es JSON"),
    };

    /// <summary>#7 — Vacío, 51 eventos, sin `eventos` o no JSON: 400 `lote_invalido` y nada guardado.</summary>
    [Theory]
    [MemberData(nameof(CuerposInvalidos))]
    public async Task Lote_invalido_responde_400_lote_invalido(string caso)
    {
        // Arrange
        using var api = new FabricaApi(bd);
        var lote = new LoteDePrueba();

        // Act
        using var respuesta = await api.CreateClient().SendAsync(lote.Peticion(Cuerpo(lote, caso)), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("lote_invalido", await ProblemaLeido.CodigoAsync(respuesta));
    }

    /// <summary>
    /// #8 — Un cuerpo de 300 KB supera el tope de 256 KB: 413 `lote_demasiado_grande`. Corre sobre **Kestrel real**:
    /// TestServer no aplica límites de cuerpo, y con Kestrel este caso respondía 500 hasta el 2026-11-21.
    /// </summary>
    [Fact]
    public async Task Cuerpo_de_300_KB_responde_413_lote_demasiado_grande()
    {
        // Arrange
        using var api = new FabricaApi(bd);
        api.UseKestrel(0);
        api.StartServer();
        var lote = new LoteDePrueba();
        var evento = lote.Evento(Guid.CreateVersion7());
        evento["relleno"] = new string('x', 300 * 1024);

        // Act
        using var respuesta = await api.CreateClient().SendAsync(lote.Peticion(LoteDePrueba.Cuerpo([evento])), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, respuesta.StatusCode);
        Assert.Equal("lote_demasiado_grande", await ProblemaLeido.CodigoAsync(respuesta));
    }

    /// <summary>
    /// #12 — Cola de capacidad 1 sin consumidores: el primer lote la ocupa y el segundo recibe
    /// 503 `saturado` con `Retry-After: 30` (ReintentarEnSegundos) y no se guarda nada de él.
    /// </summary>
    [Fact]
    public async Task Cola_llena_responde_503_saturado_con_Retry_After_y_no_guarda()
    {
        // Arrange — cola real con capacidad 1; se quitan procesador y barredor para que nadie la vacíe
        using var api = new FabricaApi(bd).WithWebHostBuilder(b =>
        {
            b.UseSetting("Ingesta:CapacidadCola", "1");
            b.ConfigureTestServices(QuitarConsumidoresDeLaCola);
        });
        var cliente = api.CreateClient();
        var lote = new LoteDePrueba();
        await EnvioLote.EnviarAsync(cliente, lote.Peticion(LoteDePrueba.Cuerpo(lote.Eventos(1))));
        var rechazados = lote.Eventos(3);

        // Act
        using var respuesta = await cliente.SendAsync(lote.Peticion(LoteDePrueba.Cuerpo(rechazados)), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, respuesta.StatusCode);
        Assert.Equal("30", Assert.Single(respuesta.Headers.GetValues("Retry-After")));
        Assert.Equal("saturado", await ProblemaLeido.CodigoAsync(respuesta));
        Assert.Empty(await RecepcionEnBd.LeerAsync(bd.CadenaSuperusuario, rechazados.Select(e => (Guid)e["eventoId"]!)));
    }

    private static void QuitarConsumidoresDeLaCola(IServiceCollection servicios)
    {
        var consumidores = new[]
        {
            typeof(KillaUru.Ingesta.Infrastructure.ProcesadorEventosBackgroundService),
            typeof(KillaUru.Ingesta.Infrastructure.BarredorBackgroundService),
        };
        foreach (var registro in servicios.Where(s => consumidores.Contains(s.ImplementationType)).ToArray())
            servicios.Remove(registro);
    }
}
