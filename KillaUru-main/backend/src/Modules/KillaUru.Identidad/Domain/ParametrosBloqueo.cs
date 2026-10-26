namespace KillaUru.Identidad.Domain;

/// <summary>Parámetros de RN-19 recibidos de la configuración, independientes del framework.</summary>
internal sealed record ParametrosBloqueo(int IntentosMaximos, int VentanaMinutos, int[] DuracionesMinutos);
