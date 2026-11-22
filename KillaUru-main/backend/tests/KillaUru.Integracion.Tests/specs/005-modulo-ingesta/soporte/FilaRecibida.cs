namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>Fila de `ingesta.evento_recibido`. Estado: 0 Recibido, 1 Procesado, 2 Descartado.</summary>
internal sealed record FilaRecibida(Guid EventoId, Guid? LoteId, short Estado, bool ConSesion);
