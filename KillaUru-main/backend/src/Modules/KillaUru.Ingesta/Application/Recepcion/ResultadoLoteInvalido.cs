using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>El cuerpo no era válido como lote (vacío, >MaxEventos, mal JSON).</summary>
internal sealed record ResultadoLoteInvalido : ResultadoLote;
