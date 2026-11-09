using KillaUru.Anomalias.Contracts;

namespace KillaUru.Mapa.Domain;

/// <summary>Consulta con caché por celdas y ETag (anónimo o conductor sin filtros).</summary>
internal sealed record PlanConsultaPorCeldas(Alcance Alcance) : PlanConsulta;
