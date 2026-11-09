using KillaUru.Anomalias.Contracts;

namespace KillaUru.Mapa.Domain;

/// <summary>Cambios desde un instante; sin caché ni ETag.</summary>
internal sealed record PlanConsultaCambios(bool RecortarDemo) : PlanConsulta;
