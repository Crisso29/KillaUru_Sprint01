using System.Globalization;
using System.Text;
using KillaUru.Anomalias.Application.Exportacion;
namespace KillaUru.Anomalias.Infrastructure.Exportacion;
/// <summary>CSV RFC 4180: texto entre comillas y números invariantes, sin materializar el dataset.</summary>
internal sealed class EscritorCsv : IEscritorExportacion
{
    public string Formato => "CSV";
    public async Task EscribirAsync(Stream destino, IAsyncEnumerable<FilaExportacion> filas, CancellationToken ct)
    {
        await using var escritor = new StreamWriter(destino, new UTF8Encoding(false), 4096, leaveOpen: true);
        await escritor.WriteLineAsync("id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte".AsMemory(), ct);
        await foreach (var f in filas.WithCancellation(ct)) await escritor.WriteLineAsync(Linea(f).AsMemory(), ct);
        await escritor.FlushAsync(ct);
    }
    private static string Linea(FilaExportacion f) => string.Join(',', new[] { f.Id.ToString(), Texto(f.Tipo),
        Numero(f.Severidad), Texto(f.Estado), Texto(f.Persistencia), Numero(f.Km), Numero(f.Latitud), Numero(f.Longitud),
        Numero(f.Reportes), f.PrimerReporte.ToString("O", CultureInfo.InvariantCulture), f.UltimoReporte.ToString("O", CultureInfo.InvariantCulture) });
    private static string Numero<T>(T valor) where T : IFormattable => valor.ToString(null, CultureInfo.InvariantCulture);
    private static string Texto(string valor) => "\"" + valor.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
