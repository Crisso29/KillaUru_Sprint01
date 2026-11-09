namespace KillaUru.Mapa.Application;

/// <summary>200 OK con cuerpo GeoJSON; ETag presente solo en el camino por celdas.</summary>
internal sealed record ResultadoMapaContenido(byte[] Cuerpo, string? ETag, bool Publico) : ResultadoMapa;
