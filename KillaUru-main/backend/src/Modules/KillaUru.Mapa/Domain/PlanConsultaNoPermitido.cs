using KillaUru.Anomalias.Contracts;

namespace KillaUru.Mapa.Domain;

/// <summary>Rol sin permiso para usar el parámetro solicitado → 403 (T-SEG20).</summary>
internal sealed record PlanConsultaNoPermitido : PlanConsulta;
