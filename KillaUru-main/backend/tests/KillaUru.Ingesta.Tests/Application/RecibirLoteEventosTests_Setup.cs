using System.Text.Json;
using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Application.Recepcion;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace KillaUru.Ingesta.Tests.Application;

internal static class RecibirLoteEventosTests_Setup
{
    internal static readonly Guid S = Guid.Parse("6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f");

    internal static JsonElement ElementoEvento(
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

    internal static LoteEntrante LoteConN(int n, bool conSesion = false, Guid? cabecera = null)
    {
        var elementos = Enumerable.Range(0, n).Select(_ => ElementoEvento()).ToList();
        return new LoteEntrante(elementos, conSesion, cabecera ?? S);
    }

    internal static (RecibirLoteEventosUseCase uc, IAnomaliasService anomalias,
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
}
