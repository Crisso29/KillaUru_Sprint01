using System.Text;
using System.Text.Json;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Contracts.Lecturas;

namespace KillaUru.Mapa.Infrastructure;

/// <summary>Serializa anomalías y colecciones en GeoJSON (data-model §2).</summary>
internal static class SerializadorGeoJson
{
    private static readonly JsonWriterOptions Opts = new() { SkipValidation = true };

    /// <summary>Bytes de las features de una celda (sin envoltorio FeatureCollection).</summary>
    internal static byte[] SerializarCelda(IReadOnlyList<AnomaliaVisible> anomalias, bool conEstado = false)
    {
        if (anomalias.Count == 0) return [];
        using var ms = new MemoryStream(512 * anomalias.Count);
        using var w  = new Utf8JsonWriter(ms, Opts);
        for (int i = 0; i < anomalias.Count; i++)
        {
            if (i > 0) ms.WriteByte((byte)',');
            EscribirFeature(w, anomalias[i], conEstado);
            w.Flush(); ms.Flush();
        }
        return ms.ToArray();
    }

    /// <summary>Ensambla la FeatureCollection completa a partir de fragmentos de celdas pre-serializados.</summary>
    internal static byte[] EnsamblarColeccion(IEnumerable<byte[]> fragmentos, DateTimeOffset generadoUtc)
    {
        using var ms = new MemoryStream(4096);
        using var w  = new Utf8JsonWriter(ms, Opts);
        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        w.WriteString("generadoUtc", generadoUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        w.WritePropertyName("features");
        w.Flush();
        ms.WriteByte((byte)'[');
        bool primero = true;
        foreach (var frag in fragmentos)
        {
            if (frag.Length == 0) continue;
            if (!primero) ms.WriteByte((byte)',');
            ms.Write(frag);
            primero = false;
        }
        ms.WriteByte((byte)']');
        ms.WriteByte((byte)'}');
        return ms.ToArray();
    }

    /// <summary>FeatureCollection para el camino filtrado o cambios.</summary>
    internal static byte[] SerializarColeccionFiltrada(IReadOnlyList<AnomaliaCambio> anomalias,
        IReadOnlyList<Guid> bajas, DateTimeOffset generadoUtc, bool conEstado)
    {
        using var ms = new MemoryStream(4096);
        using var w  = new Utf8JsonWriter(ms, Opts);
        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        w.WriteString("generadoUtc", generadoUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        w.WriteStartArray("features");
        foreach (var a in anomalias) EscribirFeatureCambio(w, a, conEstado);
        foreach (var id in bajas)   EscribirBaja(w, id);
        w.WriteEndArray();
        w.WriteEndObject();
        w.Flush();
        return ms.ToArray();
    }

    private static void EscribirFeature(Utf8JsonWriter w, AnomaliaVisible a, bool conEstado)
    {
        w.WriteStartObject();
        w.WriteString("type", "Feature");
        w.WriteString("id", a.Id.ToString());
        w.WriteStartObject("geometry");
        w.WriteString("type", "Point");
        w.WriteStartArray("coordinates");
        w.WriteNumberValue(a.Longitud);
        w.WriteNumberValue(a.Latitud);
        w.WriteEndArray();
        w.WriteEndObject();
        w.WriteStartObject("properties");
        EscribirPropiedadesComunes(w, a.Id, a.KmVia, a.Tipo.ToString(), a.Severidad,
            a.Persistencia, a.UltimaDeteccionUtc, a.VenceUtc);
        w.WriteEndObject();
        w.WriteEndObject();
    }

    private static void EscribirFeatureCambio(Utf8JsonWriter w, AnomaliaCambio a, bool conEstado)
    {
        bool esEstadoFinal = a.Estado is EstadoAnomalia.Expirada or EstadoAnomalia.Reparada or EstadoAnomalia.Descartada;
        w.WriteStartObject();
        w.WriteString("type", "Feature");
        w.WriteString("id", a.Id.ToString());
        w.WriteStartObject("geometry");
        w.WriteString("type", "Point");
        w.WriteStartArray("coordinates");
        w.WriteNumberValue(a.Longitud);
        w.WriteNumberValue(a.Latitud);
        w.WriteEndArray();
        w.WriteEndObject();
        w.WriteStartObject("properties");
        EscribirPropiedadesComunes(w, a.Id, a.KmVia, a.Tipo.ToString(), a.Severidad,
            a.Persistencia, a.UltimaDeteccionUtc, esEstadoFinal ? (DateTimeOffset?)null : a.VenceUtc);
        if (conEstado) w.WriteString("estado", a.Estado.ToString());
        w.WriteEndObject();
        w.WriteEndObject();
    }

    private static void EscribirBaja(Utf8JsonWriter w, Guid id)
    {
        w.WriteStartObject();
        w.WriteString("type", "Feature");
        w.WriteString("id", id.ToString());
        w.WriteNull("geometry");
        w.WriteStartObject("properties");
        w.WriteString("id", id.ToString());
        w.WriteBoolean("eliminada", true);
        w.WriteEndObject();
        w.WriteEndObject();
    }

    private static void EscribirPropiedadesComunes(Utf8JsonWriter w, Guid id, float kmVia,
        string tipo, int severidad, string persistencia, DateTimeOffset ultima, DateTimeOffset? vence)
    {
        w.WriteString("id", id.ToString());
        w.WriteNumber("kmVia", (double)kmVia);
        w.WriteString("tipo", tipo);
        w.WriteNumber("severidad", severidad);
        w.WriteString("persistencia", persistencia);
        w.WriteString("ultimaDeteccionUtc", ultima.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        if (vence.HasValue)
            w.WriteString("venceUtc", vence.Value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        else
            w.WriteNull("venceUtc");
    }
}
