namespace KillaUru.Integracion.Tests.Ingesta;

/// <summary>Cuerpo 200 de POST /api/v1/anomalies/batch, leído como lo haría la app (contrato 005).</summary>
internal sealed record RespuestaLoteLeida(Guid LoteId, Guid[] Aceptados, Guid[] Duplicados, RechazoLeido[] Rechazados);
