namespace KillaUru.Identidad.Domain;

/// <summary>Estado y transición de los intentos fallidos de RN-19.</summary>
internal sealed class PoliticaBloqueo
{
    public short IntentosFallidos { get; private set; }
    public short NivelBloqueo { get; private set; }
    public DateTimeOffset? VentanaFallosDesdeUtc { get; private set; }
    public DateTimeOffset? BloqueadoHasta { get; private set; }
    internal bool RegistrarFallo(DateTimeOffset ahora, ParametrosBloqueo parametros)
    {
        if (EstaBloqueado(ahora)) return false;
        if (VentanaFallosDesdeUtc is null || ahora - VentanaFallosDesdeUtc > TimeSpan.FromMinutes(parametros.VentanaMinutos))
        {
            IntentosFallidos = 0;
            VentanaFallosDesdeUtc = ahora;
        }
        IntentosFallidos++;
        if (IntentosFallidos < parametros.IntentosMaximos) return false;
        var nivel = Math.Min(NivelBloqueo, parametros.DuracionesMinutos.Length - 1);
        BloqueadoHasta = ahora.AddMinutes(parametros.DuracionesMinutos[nivel]);
        NivelBloqueo = (short)Math.Min(NivelBloqueo + 1, parametros.DuracionesMinutos.Length);
        IntentosFallidos = 0;
        VentanaFallosDesdeUtc = null;
        return true;
    }

    internal bool EstaBloqueado(DateTimeOffset ahora) => BloqueadoHasta > ahora;

    internal void RegistrarExito()
    {
        IntentosFallidos = 0;
        NivelBloqueo = 0;
        VentanaFallosDesdeUtc = null;
        BloqueadoHasta = null;
    }
}
