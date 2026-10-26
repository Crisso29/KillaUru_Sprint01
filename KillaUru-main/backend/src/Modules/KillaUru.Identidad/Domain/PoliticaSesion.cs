namespace KillaUru.Identidad.Domain;

/// <summary>Vencimiento deslizante del conductor y absoluto del administrador (FR-015).</summary>
internal static class PoliticaSesion
{
    /// <summary>Solo el administrador tiene límite absoluto; cada renovación suya no puede pasarlo.</summary>
    internal static DateTimeOffset Vencimiento(DateTimeOffset ahora, DateTimeOffset? limiteAbsoluto,
        int diasConductor, int horasAdministrador)
    {
        if (limiteAbsoluto is null) return HorasGuardadas.ALaHora(ahora.AddDays(diasConductor));
        var siguienteRenovacion = ahora.AddHours(horasAdministrador);
        var primeroQueOcurre = siguienteRenovacion < limiteAbsoluto.Value ? siguienteRenovacion : limiteAbsoluto.Value;
        return HorasGuardadas.ALaHora(primeroQueOcurre);
    }
}
