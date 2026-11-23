namespace KillaUru.Integracion.Tests.Mapa;

/// <summary>Una anomalía del GeoJSON del mapa, con lo que comprueban las pruebas de 006.</summary>
/// <param name="Estado">Solo viene para el administrador (FR-010).</param>
/// <param name="VenceNulo">D6: los estados finales llevan <c>venceUtc: null</c>.</param>
internal sealed record AnomaliaEnMapa(Guid Id, double Longitud, double Latitud, string Tipo, int Severidad,
    string? Estado, bool TieneVence, bool VenceNulo);
