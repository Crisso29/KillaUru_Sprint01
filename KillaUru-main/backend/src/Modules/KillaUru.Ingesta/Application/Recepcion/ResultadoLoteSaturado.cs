using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>La cola estaba llena; la app debe reintentar tras ReintentarEn (DA-05).</summary>
internal sealed record ResultadoLoteSaturado(TimeSpan ReintentarEn) : ResultadoLote;
