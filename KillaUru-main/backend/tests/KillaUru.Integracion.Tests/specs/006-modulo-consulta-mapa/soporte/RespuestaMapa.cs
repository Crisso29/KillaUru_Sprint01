using System.Text.Json;

namespace KillaUru.Integracion.Tests.Mapa;

/// <summary>Lee la respuesta HTTP del mapa: estado, cabeceras y las anomalías (sin las bajas de `updatedSince`).</summary>
internal sealed record RespuestaMapa(HttpResponseMessage Http, IReadOnlyList<AnomaliaEnMapa> Anomalias)
{
    internal IEnumerable<Guid> Ids => Anomalias.Select(anomalia => anomalia.Id);

    /// <summary>ADR-006: el borde (Cloudflare) puede guardarla 60 s; solo para el anónimo, igual para todos.</summary>
    internal bool EsPublicaPor60s => Http.Headers.CacheControl is { Public: true, MaxAge.TotalSeconds: 60 };

    /// <summary>FR-013: lo de un usuario con sesión nunca queda en cachés intermedias (el orden de las directivas no importa).</summary>
    internal bool EsPrivadaSinCache => Http.Headers.CacheControl is { Private: true, NoStore: true };

    internal bool TieneETag => Http.Headers.ETag is not null;

    internal static async Task<RespuestaMapa> PedirAsync(HttpClient cliente, string consulta = "")
    {
        var http = await cliente.GetAsync($"/api/v1/anomalies{consulta}", TestContext.Current.CancellationToken);
        if (!http.IsSuccessStatusCode) return new(http, []);
        using var json = JsonDocument.Parse(await http.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var anomalias = json.RootElement.GetProperty("features").EnumerateArray()
            .Where(feature => feature.GetProperty("geometry").ValueKind == JsonValueKind.Object)
            .Select(Leer).ToList();
        return new(http, anomalias);
    }

    private static AnomaliaEnMapa Leer(JsonElement feature)
    {
        var coordenadas = feature.GetProperty("geometry").GetProperty("coordinates");
        var propiedades = feature.GetProperty("properties");
        var tieneVence = propiedades.TryGetProperty("venceUtc", out var vence);
        return new(Guid.Parse(feature.GetProperty("id").GetString()!), coordenadas[0].GetDouble(), coordenadas[1].GetDouble(),
            propiedades.GetProperty("tipo").GetString()!, propiedades.GetProperty("severidad").GetInt32(),
            propiedades.TryGetProperty("estado", out var estado) ? estado.GetString() : null,
            tieneVence, tieneVence && vence.ValueKind == JsonValueKind.Null);
    }
}
