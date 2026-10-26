namespace KillaUru.Identidad.Domain;

/// <summary>Estado y transición de los intentos fallidos de RN-19.</summary>
internal sealed class PoliticaBloqueo
{
    public short IntentosFallidos { get; private set; }
    public short NivelBloqueo { get; private set; }
    public DateTimeOffset? VentanaFallosDesdeUtc { get; private set; }
    public DateTimeOffset? BloqueadoHasta { get; private set; }

    /// <summary>Devuelve true solo cuando este fallo inicia un bloqueo nuevo.</summary>
    internal bool RegistrarFallo(DateTimeOffset ahora, ParametrosBloqueo parametros)
    {
        // Mientras dura el bloqueo, los fallos no cuentan: así un atacante no puede alargarlo.
        if (EstaBloqueado(ahora)) return false;
        if (VentanaVencida(ahora, parametros)) AbrirVentana(ahora);
        IntentosFallidos++;
        if (IntentosFallidos < parametros.IntentosMaximos) return false;
        Bloquear(ahora, parametros.DuracionesMinutos);
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

    private bool VentanaVencida(DateTimeOffset ahora, ParametrosBloqueo parametros) =>
        VentanaFallosDesdeUtc is null || ahora - VentanaFallosDesdeUtc > TimeSpan.FromMinutes(parametros.VentanaMinutos);

    private void AbrirVentana(DateTimeOffset ahora)
    {
        IntentosFallidos = 0;
        VentanaFallosDesdeUtc = ahora;
    }

    /// <summary>Cada bloqueo sube un nivel; al pasar el último se repite la duración más larga.</summary>
    private void Bloquear(DateTimeOffset ahora, int[] duracionesMinutos)
    {
        var ultimoNivel = duracionesMinutos.Length - 1;
        BloqueadoHasta = ahora.AddMinutes(duracionesMinutos[Math.Min(NivelBloqueo, ultimoNivel)]);
        NivelBloqueo = (short)Math.Min(NivelBloqueo + 1, duracionesMinutos.Length);
        IntentosFallidos = 0;
        VentanaFallosDesdeUtc = null;
    }
}
