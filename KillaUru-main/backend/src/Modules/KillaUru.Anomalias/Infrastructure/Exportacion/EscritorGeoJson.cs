using System.Text.Json;
using KillaUru.Anomalias.Application.Exportacion;
namespace KillaUru.Anomalias.Infrastructure.Exportacion;
/// <summary>Serializa una feature por vez y escribe de forma asíncrona; nunca exige IO síncrono al servidor.</summary>
internal sealed class EscritorGeoJson : IEscritorExportacion
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly byte[] Inicio = """{"type":"FeatureCollection","features":["""u8.ToArray();
    private static readonly byte[] Fin = "]}"u8.ToArray();
    private static readonly byte[] Separador = ","u8.ToArray();
    public string Formato => "GeoJSON";
    public async Task EscribirAsync(Stream destino, IAsyncEnumerable<FilaExportacion> filas, CancellationToken ct)
    {
        await destino.WriteAsync(Inicio, ct); var primero = true;
        await foreach (var f in filas.WithCancellation(ct))
        {
            if (!primero) await destino.WriteAsync(Separador, ct);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(new { type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { f.Longitud, f.Latitud } }, properties = f }, Json);
            await destino.WriteAsync(bytes, ct); primero = false;
        }
        await destino.WriteAsync(Fin, ct);
    }
}
