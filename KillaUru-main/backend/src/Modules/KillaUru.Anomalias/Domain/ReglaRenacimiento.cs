namespace KillaUru.Anomalias.Domain;
/// <summary>RN-07 distingue vencimiento de reparación; nunca reactiva una descartada automáticamente.</summary>
internal static class ReglaRenacimiento
{
    internal static bool Aplica(EstadoAnomalia estado, DateTimeOffset detectado, DateTimeOffset vence, DateTimeOffset desde) =>
        estado == EstadoAnomalia.Expirada && detectado > vence || estado == EstadoAnomalia.Reparada && detectado > desde;
}
