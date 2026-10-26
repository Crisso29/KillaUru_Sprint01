namespace KillaUru.Anomalias.Contracts.Lecturas;
/// <summary>Estado actual para sincronización incremental; Mapa aplica el recorte por rol.</summary>
public sealed record AnomaliaCambio(Guid Id, TipoAnomalia Tipo, int Severidad, string Persistencia,
    double Latitud, double Longitud, float KmVia, long Celda, DateTimeOffset UltimaDeteccionUtc,
    DateTimeOffset VenceUtc, EstadoAnomalia Estado, DateTimeOffset ActualizadaUtc);
