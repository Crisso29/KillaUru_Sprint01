using System.Text.Json;
using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>
/// Deserializa un JsonElement de forma tolerante: un campo ausente o de tipo incorrecto produce
/// null en ese campo sin lanzar excepción; nunca falla el lote entero (FR-002, R-001).
/// </summary>
internal static class LectorEvento
{
    /// <summary>Lee el elemento y devuelve (eventoId?, EventoReportado con nulls donde faltó el campo).</summary>
    internal static (Guid? Id, EventoReportado? Evento) Leer(JsonElement elemento, int indice)
    {
        var eventoId = LeerGuid(elemento, "eventoId");
        var seudonimo = LeerGuid(elemento, "seudonimo");
        var evento = new EventoReportado(
            EventoId: eventoId ?? Guid.Empty,
            DetectadoUtc: LeerDateTimeOffset(elemento, "detectadoUtc"),
            Latitud: LeerDouble(elemento, "latitud"),
            Longitud: LeerDouble(elemento, "longitud"),
            PrecisionMetros: LeerDouble(elemento, "precisionMetros"),
            Tipo: LeerString(elemento, "tipo"),
            Severidad: LeerInt(elemento, "severidad"),
            Confidence: LeerDouble(elemento, "confidence"),
            ModeloVersion: LeerString(elemento, "modeloVersion"),
            Seudonimo: seudonimo);
        return (eventoId, evento);
    }

    /// <summary>Devuelve un JsonDocument con el payload sin seudónimo ni eventoId (FR-008).</summary>
    internal static JsonDocument ObtenerPayloadSinSeudonimo(JsonElement elemento)
    {
        var campos = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in elemento.EnumerateObject())
        {
            if (prop.Name.Equals("seudonimo", StringComparison.OrdinalIgnoreCase)) continue;
            if (prop.Name.Equals("eventoId", StringComparison.OrdinalIgnoreCase)) continue;
            campos[prop.Name] = prop.Value;
        }
        var json = JsonSerializer.Serialize(campos);
        return JsonDocument.Parse(json);
    }

    private static Guid? LeerGuid(JsonElement e, string nombre)
    {
        if (!e.TryGetProperty(nombre, out var prop)) return null;
        if (prop.ValueKind != JsonValueKind.String) return null;
        return Guid.TryParse(prop.GetString(), out var v) ? v : null;
    }

    private static DateTimeOffset? LeerDateTimeOffset(JsonElement e, string nombre)
    {
        if (!e.TryGetProperty(nombre, out var prop)) return null;
        if (prop.ValueKind != JsonValueKind.String) return null;
        return DateTimeOffset.TryParse(prop.GetString(), out var v) ? v : null;
    }

    private static double? LeerDouble(JsonElement e, string nombre)
    {
        if (!e.TryGetProperty(nombre, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.Number && prop.TryGetDouble(out var v) ? v : null;
    }

    private static int? LeerInt(JsonElement e, string nombre)
    {
        if (!e.TryGetProperty(nombre, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var v) ? v : null;
    }

    private static string? LeerString(JsonElement e, string nombre)
    {
        if (!e.TryGetProperty(nombre, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }
}
