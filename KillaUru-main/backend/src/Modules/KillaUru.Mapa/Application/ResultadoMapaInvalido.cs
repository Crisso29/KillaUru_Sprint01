namespace KillaUru.Mapa.Application;

/// <summary>400 — parámetro inválido o combinación incompatible.</summary>
internal sealed record ResultadoMapaInvalido(string Codigo) : ResultadoMapa;
