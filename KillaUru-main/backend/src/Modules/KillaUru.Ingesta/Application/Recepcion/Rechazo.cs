using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Evento que no se guardó; Id es null cuando faltó el eventoId en el cuerpo (D5).</summary>
internal sealed record Rechazo(Guid? Id, int Indice, MotivoDescarte Motivo);
