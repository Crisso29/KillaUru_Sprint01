using System.Text.Json.Serialization;
namespace KillaUru.Anomalias.Application.Exportacion;
/// <summary>Esquema público del dataset; excluye seudónimos y cuentas por construcción.</summary>
internal sealed record FilaExportacion(Guid Id, string Tipo, int Severidad, string Estado, string Persistencia,
    float Km, double Latitud, double Longitud, int Reportes,
    [property: JsonPropertyName("primer_reporte")] DateTimeOffset PrimerReporte,
    [property: JsonPropertyName("ultimo_reporte")] DateTimeOffset UltimoReporte);
