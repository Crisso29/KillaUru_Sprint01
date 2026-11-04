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
/// RecibirLoteEventosUseCase valida el lote, genera el loteId, evalúa cada evento con
/// IAnomaliasService, guarda los aceptados y avisa por la cola (FR-003…FR-010, FR-023…FR-025).
/// Spec 005 · T010.
/// </summary>
public sealed class RecibirLoteEventosUseCaseTests
{
    private static readonly Guid S = Guid.Parse("6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f");

    private static JsonElement ElementoEvento(
        string? eventoId = null, string? seudonimo = null, int? severidad = 4)
    {
        var id = eventoId ?? Guid.NewGuid().ToString();
        var s = seudonimo ?? S.ToString();
        var sev = severidad?.ToString() ?? "null";
        var json = $$"""
            {
              "eventoId": "{{id}}",
              "detectadoUtc": "2026-10-20T14:32:05Z",
              "latitud": -13.2105, "longitud": -74.2581,
              "precisionMetros": 6.5, "tipo": "Bache",
              "severidad": {{sev}}, "confidence": 0.91,
              "modeloVersion": "1.2.0", "seudonimo": "{{s}}"
            }
            """;
        return JsonDocument.Parse(json).RootElement;
    }

    private static LoteEntrante LoteConN(int n, bool conSesion = false, Guid? cabecera = null)
    {
        var elementos = Enumerable.Range(0, n).Select(_ => ElementoEvento()).ToList();
        return new LoteEntrante(elementos, conSesion, cabecera ?? S);
    }

    private static (RecibirLoteEventosUseCase uc, IAnomaliasService anomalias,
        IRecepcionEventos recepcion, IColaLotes cola, IMetricas metricas)
    Crear(bool hayCapacidad = true, int maxPorLote = 50)
    {
        var anomalias = Substitute.For<IAnomaliasService>();
        var recepcion = Substitute.For<IRecepcionEventos>();
        var cola = Substitute.For<IColaLotes>();
        var metricas = Substitute.For<IMetricas>();
        var opciones = Options.Create(new IngestaOptions { MaxEventosPorLote = maxPorLote });

        anomalias.EvaluarIntrinseca(Arg.Any<EventoReportado>(), Arg.Any<DateTimeOffset>())
                 .Returns((MotivoDescarte?)null);
        cola.HayCapacidad().Returns(hayCapacidad);
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
                 .Returns(ci => ((LoteAceptado)ci[0]).Aceptados.Select(e => e.EventoId).ToHashSet());

        var uc = new RecibirLoteEventosUseCase(anomalias, recepcion, cola, metricas, opciones, TimeProvider.System);
        return (uc, anomalias, recepcion, cola, metricas);
    }

    /// <summary>Un lote vacío devuelve Invalido sin guardar nada (FR-003).</summary>
    [Fact]
    public async Task Lote_vacio_devuelve_invalido()
    {
        // Arrange
        var (uc, _, recepcion, _, _) = Crear();
        var lote = new LoteEntrante([], false, null);

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        Assert.IsType<ResultadoLote.Invalido>(resultado);
        await recepcion.DidNotReceive().InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Un lote de 51 eventos devuelve Invalido sin guardar nada (FR-003).</summary>
    [Fact]
    public async Task Lote_con_mas_de_50_eventos_devuelve_invalido()
    {
        // Arrange
        var (uc, _, recepcion, _, _) = Crear();
        var lote = LoteConN(51);

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        Assert.IsType<ResultadoLote.Invalido>(resultado);
        await recepcion.DidNotReceive().InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Sin capacidad en la cola devuelve Saturado antes de guardar (FR-009).</summary>
    [Fact]
    public async Task Sin_capacidad_devuelve_saturado_sin_insertar()
    {
        // Arrange
        var (uc, _, recepcion, _, _) = Crear(hayCapacidad: false);
        var lote = LoteConN(1);

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        Assert.IsType<ResultadoLote.Saturado>(resultado);
        await recepcion.DidNotReceive().InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>());
    }

    /// <summary>El loteId en la respuesta es UUIDv7 generado por el servidor (D3, FR-025).</summary>
    [Fact]
    public async Task LoteId_en_respuesta_es_UUIDv7_del_servidor()
    {
        // Arrange
        var (uc, _, _, _, _) = Crear();
        var lote = LoteConN(1);

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        var aceptado = Assert.IsType<ResultadoLote.Aceptado>(resultado);
        Assert.NotEqual(Guid.Empty, aceptado.LoteId);
        // UUIDv7: versión 7 en bits 76-73 del byte 6
        var bytes = aceptado.LoteId.ToByteArray();
        var version = (bytes[7] >> 4) & 0xF;
        Assert.Equal(7, version);
    }

    /// <summary>Todos los eventos del lote aparecen en exactamente una lista (SC-004, FR-006).</summary>
    [Fact]
    public async Task Todos_los_eventos_aparecen_en_exactamente_una_lista()
    {
        // Arrange — un evento bueno, uno con severidad nula (EsquemaInvalido)
        var (uc, anomalias, recepcion, _, _) = Crear();
        var idBueno = Guid.NewGuid();
        var idMalo = Guid.NewGuid();
        var elementos = new List<JsonElement>
        {
            ElementoEvento(eventoId: idBueno.ToString()),
            ElementoEvento(eventoId: idMalo.ToString(), severidad: null)
        };
        anomalias.EvaluarIntrinseca(
            Arg.Is<EventoReportado>(e => e.EventoId == idMalo), Arg.Any<DateTimeOffset>())
            .Returns(MotivoDescarte.EsquemaInvalido);
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid> { idBueno });
        var lote = new LoteEntrante(elementos, false, S);

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        var aceptado = Assert.IsType<ResultadoLote.Aceptado>(resultado);
        var todosLosIds = aceptado.Aceptados.Union(aceptado.Duplicados)
            .Union(aceptado.Rechazados.Where(r => r.Id.HasValue).Select(r => r.Id!.Value))
            .ToList();
        Assert.Equal(2, todosLosIds.Count);
        Assert.Contains(idBueno, todosLosIds);
        Assert.Contains(idMalo, todosLosIds);
    }

    /// <summary>Un evento sin eventoId sale en rechazados con id=null e indice correcto (D5, FR-023).</summary>
    [Fact]
    public async Task Evento_sin_eventoId_sale_rechazado_con_id_null_e_indice()
    {
        // Arrange
        var (uc, _, _, _, _) = Crear();
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

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        var aceptado = Assert.IsType<ResultadoLote.Aceptado>(resultado);
        var rechazo = Assert.Single(aceptado.Rechazados);
        Assert.Null(rechazo.Id);
        Assert.Equal(0, rechazo.Indice);
        Assert.Equal(MotivoDescarte.EsquemaInvalido, rechazo.Motivo);
    }

    /// <summary>Evento con seudónimo distinto al del lote sale como SeudonimoInconsistente (FR-024, D2).</summary>
    [Fact]
    public async Task Evento_con_seudonimo_diferente_es_inconsistente()
    {
        // Arrange
        var (uc, _, _, _, _) = Crear();
        var otroConductor = Guid.NewGuid();
        var elementos = new List<JsonElement>
        {
            ElementoEvento(seudonimo: otroConductor.ToString())
        };
        var lote = new LoteEntrante(elementos, false, S);  // lote fijado en S

        // Act
        var resultado = await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        var aceptado = Assert.IsType<ResultadoLote.Aceptado>(resultado);
        var rechazo = Assert.Single(aceptado.Rechazados);
        Assert.Equal(MotivoDescarte.SeudonimoInconsistente, rechazo.Motivo);
    }

    /// <summary>Las métricas de lote y de rechazo por motivo se registran (FR-010).</summary>
    [Fact]
    public async Task Metricas_de_lote_y_rechazo_se_registran()
    {
        // Arrange
        var (uc, anomalias, recepcion, _, metricas) = Crear();
        var idMalo = Guid.NewGuid();
        var elementos = new List<JsonElement>
        {
            ElementoEvento(),
            ElementoEvento(eventoId: idMalo.ToString())
        };
        anomalias.EvaluarIntrinseca(
            Arg.Is<EventoReportado>(e => e.EventoId == idMalo), Arg.Any<DateTimeOffset>())
            .Returns(MotivoDescarte.FueraDelCorredor);
        var id1 = ((List<JsonElement>)elementos)[0]
            .GetProperty("eventoId").GetGuid();
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid> { id1 });
        var lote = new LoteEntrante(elementos, false, S);

        // Act
        await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert
        metricas.Received(1).RegistrarLote(1, 0, 1);
        metricas.Received(1).RegistrarRechazo(nameof(MotivoDescarte.FueraDelCorredor));
    }

    /// <summary>Todos los eventos del lote comparten la misma recibidoUtc (FR-004).</summary>
    [Fact]
    public async Task Todos_los_eventos_comparten_la_misma_recibidoUtc()
    {
        // Arrange
        DateTimeOffset? recibidoCapturado = null;
        var anomalias = Substitute.For<IAnomaliasService>();
        var recepcion = Substitute.For<IRecepcionEventos>();
        var cola = Substitute.For<IColaLotes>();
        var metricas = Substitute.For<IMetricas>();
        var opciones = Options.Create(new IngestaOptions());
        anomalias.EvaluarIntrinseca(Arg.Any<EventoReportado>(), Arg.Do<DateTimeOffset>(r => recibidoCapturado = r))
                 .Returns((MotivoDescarte?)null);
        cola.HayCapacidad().Returns(true);
        recepcion.InsertarAsync(Arg.Any<LoteAceptado>(), Arg.Any<CancellationToken>())
                 .Returns(ci => ((LoteAceptado)ci[0]).Aceptados.Select(e => e.EventoId).ToHashSet());
        var uc = new RecibirLoteEventosUseCase(anomalias, recepcion, cola, metricas, opciones, TimeProvider.System);
        var lote = LoteConN(3);

        // Act
        await uc.ProcesarAsync(lote, CancellationToken.None);

        // Assert — EvaluarIntrinseca se llamó 3 veces y capturamos que el recibido fue consistente
        anomalias.Received(3).EvaluarIntrinseca(Arg.Any<EventoReportado>(), Arg.Is<DateTimeOffset>(r => r == recibidoCapturado));
    }
}
