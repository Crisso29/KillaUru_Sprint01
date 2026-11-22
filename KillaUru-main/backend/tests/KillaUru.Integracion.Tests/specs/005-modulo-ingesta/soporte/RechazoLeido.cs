namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>Evento no guardado: su eventoId (null si no traía uno válido), su posición 0-based y el motivo (D5).</summary>
internal sealed record RechazoLeido(Guid? Id, int Indice, string Motivo);
