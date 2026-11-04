using System.Text.Json;
using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>
/// Lote listo para insertar: eventos únicos (repetidos en el lote ya filtrados) con sus payloads
/// sin seudónimo (FR-006, FR-008). ComoParametros() prepara los arreglos para el INSERT con unnest.
/// </summary>
internal sealed class LoteAceptado
{
    internal Guid LoteId { get; }
    internal DateTimeOffset RecibidoUtc { get; }
    internal bool ConSesion { get; }
    internal IReadOnlyList<EventoReportado> Aceptados { get; }
    internal IReadOnlyList<Guid> RepetidosEnLote { get; }

    private LoteAceptado(Guid loteId, DateTimeOffset recibidoUtc, bool conSesion,
        IReadOnlyList<EventoReportado> aceptados, IReadOnlyList<Guid> repetidos)
    {
        LoteId = loteId;
        RecibidoUtc = recibidoUtc;
        ConSesion = conSesion;
        Aceptados = aceptados;
        RepetidosEnLote = repetidos;
    }

    /// <summary>Separa eventos únicos de repetidos dentro del lote (R-003).</summary>
    internal static LoteAceptado De(Guid loteId, DateTimeOffset recibidoUtc, bool conSesion,
        IEnumerable<EventoReportado> candidatos, IReadOnlyList<Rechazo> yaRechazados)
    {
        var vistos = new HashSet<Guid>();
        var aceptados = new List<EventoReportado>();
        var repetidos = new List<Guid>();
        foreach (var e in candidatos)
        {
            if (!vistos.Add(e.EventoId)) repetidos.Add(e.EventoId);
            else aceptados.Add(e);
        }
        return new LoteAceptado(loteId, recibidoUtc, conSesion, aceptados, repetidos);
    }

    /// <summary>
    /// Devuelve los arreglos paralelos para el INSERT con unnest.
    /// El payload de cada evento se serializa sin seudónimo (FR-008).
    /// </summary>
    internal (Guid LoteId, DateTimeOffset RecibidoUtc, bool ConSesion,
              Guid[] EventoIds, JsonDocument[] Payloads, DateTimeOffset[] DetectadosUtc) ComoParametros()
    {
        var ids = Aceptados.Select(e => e.EventoId).ToArray();
        var payloads = Aceptados.Select(e => BuildPayload(e)).ToArray();
        var detectados = Aceptados.Select(e => e.DetectadoUtc ?? RecibidoUtc).ToArray();
        return (LoteId, RecibidoUtc, ConSesion, ids, payloads, detectados);
    }

    private static JsonDocument BuildPayload(EventoReportado e)
    {
        var dict = new Dictionary<string, object?>
        {
            ["detectadoUtc"] = e.DetectadoUtc,
            ["latitud"] = e.Latitud,
            ["longitud"] = e.Longitud,
            ["precisionMetros"] = e.PrecisionMetros,
            ["tipo"] = e.Tipo,
            ["severidad"] = e.Severidad,
            ["confidence"] = e.Confidence,
            ["modeloVersion"] = e.ModeloVersion,
        };
        return JsonDocument.Parse(JsonSerializer.Serialize(dict));
    }
}
