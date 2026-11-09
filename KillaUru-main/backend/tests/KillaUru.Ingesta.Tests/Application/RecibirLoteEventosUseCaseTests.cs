using KillaUru.Ingesta.Application;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Application.Recepcion;
using NSubstitute;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// RecibirLoteEventosUseCase — validación del lote entrante (FR-003, FR-009, FR-025).
/// Spec 005 · T010.
/// </summary>
public sealed class RecibirLoteEventosUseCaseTests
{
    [Fact]
    public async Task Lote_vacio_devuelve_invalido()
    {
        var (uc, _, recepcion, _, _) = RecibirLoteEventosTests_Setup.Crear();
        var lote = new LoteEntrante([], false, null);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        Assert.IsType<ResultadoLoteInvalido>(resultado);
        await recepcion.DidNotReceive().InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lote_con_mas_de_50_eventos_devuelve_invalido()
    {
        var (uc, _, recepcion, _, _) = RecibirLoteEventosTests_Setup.Crear();
        var lote = RecibirLoteEventosTests_Setup.LoteConN(51);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        Assert.IsType<ResultadoLoteInvalido>(resultado);
        await recepcion.DidNotReceive().InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sin_capacidad_devuelve_saturado_sin_insertar()
    {
        var (uc, _, recepcion, _, _) = RecibirLoteEventosTests_Setup.Crear(hayCapacidad: false);
        var lote = RecibirLoteEventosTests_Setup.LoteConN(1);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        Assert.IsType<ResultadoLoteSaturado>(resultado);
        await recepcion.DidNotReceive().InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>());
    }

    /// <summary>El loteId en la respuesta es UUIDv7 generado por el servidor (D3, FR-025).</summary>
    [Fact]
    public async Task LoteId_en_respuesta_es_UUIDv7_del_servidor()
    {
        var (uc, _, _, _, _) = RecibirLoteEventosTests_Setup.Crear();
        var lote = RecibirLoteEventosTests_Setup.LoteConN(1);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        var aceptado = Assert.IsType<ResultadoLoteAceptado>(resultado);
        Assert.NotEqual(Guid.Empty, aceptado.LoteId);
        var bytes = aceptado.LoteId.ToByteArray();
        var version = (bytes[7] >> 4) & 0xF;
        Assert.Equal(7, version);
    }
}
