namespace KillaUru.Anomalias.Domain;
/// <summary>Plazos de RN-07; los estados finales conservan la fecha persistida anterior.</summary>
internal static class ReglasAnomalia
{
    internal static TimeSpan Caducidad(EstadoAnomalia estado, EstadoPersistencia persistencia, AnomaliasOptions opciones) =>
        estado == EstadoAnomalia.PendienteConfirmacion ? TimeSpan.FromHours(opciones.CaducidadPendienteHoras)
        : persistencia == EstadoPersistencia.Persistente ? TimeSpan.FromDays(opciones.CaducidadPersistenteDias)
        : TimeSpan.FromHours(opciones.CaducidadEpisodicaHoras);
}
