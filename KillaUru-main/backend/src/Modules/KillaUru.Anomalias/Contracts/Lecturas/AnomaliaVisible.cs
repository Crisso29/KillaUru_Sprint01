using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Contracts.Lecturas;
/// <summary>Proyección activa para Mapa, sin reportes ni identificadores de dispositivos.</summary>
public sealed record AnomaliaVisible(Guid Id, TipoAnomalia Tipo, int Severidad, string Persistencia,
    double Latitud, double Longitud, float KmVia, long Celda, DateTimeOffset UltimaDeteccionUtc, DateTimeOffset VenceUtc);
