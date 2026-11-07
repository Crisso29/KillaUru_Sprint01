
namespace KillaUru.Mapa.Domain;

/// <summary>Consulta directa a Anomalías con filtros opcionales; sin caché ni ETag.</summary>
internal sealed record PlanConsultaFiltrada(EstadoMapa[] Estados, bool RecortarDemo) : PlanConsulta;
