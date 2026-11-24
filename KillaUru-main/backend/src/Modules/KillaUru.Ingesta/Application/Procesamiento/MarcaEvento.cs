using KillaUru.Ingesta.Domain;
namespace KillaUru.Ingesta.Application.Procesamiento;

/// <summary>Resultado de intentar procesar un evento: id, estado final y motivo si fue descartado (DT-01).</summary>
internal sealed record MarcaEvento(Guid EventoId, EstadoRecepcion Estado, short? Motivo);
