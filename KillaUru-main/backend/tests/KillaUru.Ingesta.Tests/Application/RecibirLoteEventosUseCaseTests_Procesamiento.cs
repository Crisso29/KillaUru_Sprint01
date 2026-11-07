using System.Text.Json;
using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Application.Recepcion;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// RecibirLoteEventosUseCase — procesamiento: partición, rechazos y métricas
/// (FR-006, FR-023, FR-024, FR-010, FR-004). Spec 005 · T010.
/// </summary>
public sealed class RecibirLoteEventosUseCaseTests_Procesamiento
{
    private static readonly Guid S = RecibirLoteEventosTests_Setup.S;

    [Fact]
    public async Task Todos_los_eventos_aparecen_en_exactamente_una_lista()
    {
        var (uc, anomalias, recepcion, _, _) = RecibirLoteEventosTests_Setup.Crear();
        var idBueno = Guid.NewGuid();
        var idMalo = Guid.NewGuid();
        var elementos = new List<JsonElement>
        {
            RecibirLoteEventosTests_Setup.ElementoEvento(eventoId: idBueno.ToString()),
            RecibirLoteEventosTests_Setup.ElementoEvento(eventoId: idMalo.ToString(), severidad: null)
        };
        anomalias.EvaluarIntrinseca(
            Arg.Is<EventoReportado>(e => e.EventoId == idMalo), Arg.Any<DateTimeOffset>())
            .Returns(MotivoDescarte.EsquemaInvalido);
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid> { idBueno });
        var lote = new LoteEntrante(elementos, false, S);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        var aceptado = Assert.IsType<ResultadoLoteAceptado>(resultado);
        var todosLosIds = aceptado.Aceptados.Union(aceptado.Duplicados)
            .Union(aceptado.Rechazados.Where(r => r.Id.HasValue).Select(r => r.Id!.Value))
            .ToList();
        Assert.Equal(2, todosLosIds.Count);
        Assert.Contains(idBueno, todosLosIds);
        Assert.Contains(idMalo, todosLosIds);
    }

    /// <summary>Evento sin eventoId sale en rechazados con id=null e índice correcto (D5, FR-023).</summary>
    [Fact]
    public async Task Evento_sin_eventoId_sale_rechazado_con_id_null_e_indice()
    {
        var (uc, _, _, _, _) = RecibirLoteEventosTests_Setup.Crear();
        var json = """
            {
              "detectadoUtc": "2026-10-20T14:32:05Z",
              "latitud": -13.2105, "longitud": -74.2581,
              "precisionMetros": 6.5, "tipo": "Bache", "severidad": 4,
              "confidence": 0.91, "modeloVersion": "1.2.0",
              "seudonimo": "6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f"
            }
            """;
        var elementos = new List<JsonElement> { JsonDocument.Parse(json).RootElement };
        var lote = new LoteEntrante(elementos, false, S);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        var aceptado = Assert.IsType<ResultadoLoteAceptado>(resultado);
        var rechazo = Assert.Single(aceptado.Rechazados);
        Assert.Null(rechazo.Id);
        Assert.Equal(0, rechazo.Indice);
        Assert.Equal(MotivoDescarte.EsquemaInvalido, rechazo.Motivo);
    }

    [Fact]
    public async Task Evento_con_seudonimo_diferente_es_inconsistente()
    {
        var (uc, _, _, _, _) = RecibirLoteEventosTests_Setup.Crear();
        var otroConductor = Guid.NewGuid();
        var elementos = new List<JsonElement>
        {
            RecibirLoteEventosTests_Setup.ElementoEvento(seudonimo: otroConductor.ToString())
        };
        var lote = new LoteEntrante(elementos, false, S);
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);
        var aceptado = Assert.IsType<ResultadoLoteAceptado>(resultado);
        var rechazo = Assert.Single(aceptado.Rechazados);
        Assert.Equal(MotivoDescarte.SeudonimoInconsistente, rechazo.Motivo);
    }

    [Fact]
    public async Task Metricas_de_lote_y_rechazo_se_registran()
    {
        var (uc, anomalias, recepcion, _, metricas) = RecibirLoteEventosTests_Setup.Crear();
        var idMalo = Guid.NewGuid();
        var elementos = new List<JsonElement>
        {
            RecibirLoteEventosTests_Setup.ElementoEvento(),
            RecibirLoteEventosTests_Setup.ElementoEvento(eventoId: idMalo.ToString())
        };
        anomalias.EvaluarIntrinseca(
            Arg.Is<EventoReportado>(e => e.EventoId == idMalo), Arg.Any<DateTimeOffset>())
            .Returns(MotivoDescarte.FueraDelCorredor);
        var id1 = ((List<JsonElement>)elementos)[0].GetProperty("eventoId").GetGuid();
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid> { id1 });
        var lote = new LoteEntrante(elementos, false, S);
        await uc.ProcesarAsync(lote, CancellationToken.None);
        metricas.Received(1).RegistrarLote(1, 0, 1);
        metricas.Received(1).RegistrarRechazo(nameof(MotivoDescarte.FueraDelCorredor));
    }

    /// <summary>Todos los eventos del lote comparten la misma recibidoUtc (FR-004).</summary>
    [Fact]
    public async Task Todos_los_eventos_comparten_la_misma_recibidoUtc()
    {
        DateTimeOffset? recibidoCapturado = null;
        var anomalias = Substitute.For<IAnomaliasService>();
        var recepcion = Substitute.For<IRecepcionEventos>();
        var cola = Substitute.For<IColaLotes>();
        var metricas = Substitute.For<IMetricas>();
        var opciones = Options.Create(new IngestaOptions());
        anomalias.EvaluarIntrinseca(
            Arg.Any<EventoReportado>(),
            Arg.Do<DateTimeOffset>(r => recibidoCapturado = r))
                 .Returns((MotivoDescarte?)null);
        cola.HayCapacidad().Returns(true);
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
                 .Returns(ci => ((LoteAceptado)ci[0]).Aceptados.Select(e => e.EventoId).ToHashSet());
        var uc = new RecibirLoteEventosUseCase(anomalias, recepcion, cola, metricas, opciones, TimeProvider.System);
        var lote = RecibirLoteEventosTests_Setup.LoteConN(3);
        await uc.ProcesarAsync(lote, CancellationToken.None);
        anomalias.Received(3).EvaluarIntrinseca(
            Arg.Any<EventoReportado>(),
            Arg.Is<DateTimeOffset>(r => r == recibidoCapturado));
    }
}
