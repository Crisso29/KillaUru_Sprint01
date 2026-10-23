namespace KillaUru.Shared.Eventos;

/// <summary>Suscriptor de un tipo de evento (patrón Observer). El publicador no lo conoce.</summary>
/// <typeparam name="TEvento">Evento que atiende.</typeparam>
public interface IManejador<in TEvento>
    where TEvento : IEventoDominio
{
    /// <summary>Reacciona al evento.</summary>
    /// <param name="evento">Hecho ocurrido.</param>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Tarea de la reacción.</returns>
    Task ManejarAsync(TEvento evento, CancellationToken ct);
}
