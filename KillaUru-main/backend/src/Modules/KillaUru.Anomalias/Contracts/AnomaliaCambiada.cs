using KillaUru.Shared.Domain;
using KillaUru.Shared.Eventos;
namespace KillaUru.Anomalias.Contracts;
/// <summary>Invalidación de la celda fija, emitida exclusivamente después del commit.</summary>
public sealed record AnomaliaCambiada(Guid AnomaliaId, Celda Celda) : IEventoDominio;
