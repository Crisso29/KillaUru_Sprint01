using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>
/// Arma lotes que pasan todas las reglas de recepción (RN-11): sobre la vía sintética de
/// <see cref="Infraestructura.FabricaApi"/>, confidence 0,91, precisión 6,5 m y detectados hace 1 minuto.
/// Cada instancia es una instalación distinta: su propio seudónimo y su propia posición.
/// </summary>
internal sealed class LoteDePrueba
{
    internal const string Ruta = "/api/v1/anomalies/batch";

    private static int secuencia;

    // 004 reparte sus escenarios desde lon -76 hacia el este; 005 va desde -75,05 hacia el oeste,
    // así las anomalías que crea el procesamiento real no se agrupan con las de otras pruebas.
    private readonly double longitud = -75.05 - (Interlocked.Increment(ref secuencia) * 0.002);

    /// <summary>Seudónimo rotativo de esta instalación; uno por lote (D2).</summary>
    internal Guid Seudonimo { get; } = Guid.NewGuid();

    /// <summary>Un evento válido con el <paramref name="eventoId"/> indicado.</summary>
    internal JsonObject Evento(Guid eventoId) => new()
    {
        ["eventoId"] = eventoId,
        // El host de FabricaApi usa el reloj del sistema: «hace 1 minuto» queda dentro de los 7 días de RN-11.
        ["detectadoUtc"] = TimeProvider.System.GetUtcNow().AddMinutes(-1),
        ["latitud"] = -13.0,
        ["longitud"] = longitud,
        ["precisionMetros"] = 6.5,
        ["tipo"] = "Bache",
        ["severidad"] = 3,
        ["confidence"] = 0.91,
        ["modeloVersion"] = "1.2.0",
        ["seudonimo"] = Seudonimo,
    };

    /// <summary><paramref name="cantidad"/> eventos válidos, cada uno con un eventoId nuevo.</summary>
    internal JsonObject[] Eventos(int cantidad) =>
        Enumerable.Range(0, cantidad).Select(_ => Evento(Guid.CreateVersion7())).ToArray();

    /// <summary>Cuerpo `{ "eventos": [...] }`; con <paramref name="loteId"/> simula un cliente antiguo (D3).</summary>
    internal static JsonContent Cuerpo(IEnumerable<JsonNode> eventos, Guid? loteId = null)
    {
        var cuerpo = new JsonObject { ["eventos"] = new JsonArray(eventos.Select(e => e.DeepClone()).ToArray()) };
        if (loteId is not null) cuerpo["loteId"] = loteId;
        return JsonContent.Create(cuerpo);
    }

    /// <summary>Petición POST al lote con la cabecera de seudónimo de esta instalación.</summary>
    internal HttpRequestMessage Peticion(HttpContent cuerpo, string? token = null)
    {
        var peticion = Infraestructura.Peticiones.Post(Ruta, token, Seudonimo.ToString());
        peticion.Content = cuerpo;
        return peticion;
    }
}
