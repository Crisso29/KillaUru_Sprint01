namespace KillaUru.Anomalias.Domain;

/// <summary>Plazos de RN-07; los estados finales conservan la fecha persistida anterior.</summary>
internal static class ReglasAnomalia
{
    /// <summary>
    /// Pendiente: 72 h para que llegue el segundo vehículo. Publicada: 7 días si es persistente
    /// (vuelve a verse en días distintos) o 72 h si fue episódica (FR-017).
    /// </summary>
    internal static TimeSpan Caducidad(EstadoAnomalia estado, EstadoPersistencia persistencia, AnomaliasOptions opciones) =>
        (estado, persistencia) switch
        {
            (EstadoAnomalia.PendienteConfirmacion, _) => TimeSpan.FromHours(opciones.CaducidadPendienteHoras),
            (_, EstadoPersistencia.Persistente) => TimeSpan.FromDays(opciones.CaducidadPersistenteDias),
            _ => TimeSpan.FromHours(opciones.CaducidadEpisodicaHoras),
        };
}
