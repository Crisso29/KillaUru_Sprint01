namespace KillaUru.Anomalias.Domain;

/// <summary>RN-07 distingue vencimiento de reparación; nunca reactiva una descartada automáticamente.</summary>
internal static class ReglaRenacimiento
{
    /// <summary>
    /// Una Expirada renace si alguien la detecta después de su vencimiento; una Reparada, si la
    /// detectan después de la reparación (el bache volvió). Ambos límites son exclusivos (FR-019).
    /// </summary>
    internal static bool Aplica(EstadoAnomalia estado, DateTimeOffset detectado, DateTimeOffset vence, DateTimeOffset desde)
    {
        var expiradaVueltaAVer = estado == EstadoAnomalia.Expirada && detectado > vence;
        var reparadaVueltaAVer = estado == EstadoAnomalia.Reparada && detectado > desde;
        return expiradaVueltaAVer || reparadaVueltaAVer;
    }
}
