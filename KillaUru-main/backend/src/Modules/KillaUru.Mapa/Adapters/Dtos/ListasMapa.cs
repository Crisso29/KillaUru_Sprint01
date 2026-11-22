using KillaUru.Anomalias.Contracts;

namespace KillaUru.Mapa.Adapters.Dtos;

/// <summary>Listas de `severidad` y `estado` ya convertidas; null en una lista significa «sin ese filtro».</summary>
internal sealed record ListasMapa(int[]? Severidades, EstadoAnomalia[]? Estados);
