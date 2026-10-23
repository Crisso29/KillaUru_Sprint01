namespace KillaUru.Shared.Eventos;

/// <summary>Publica hechos de dominio a sus suscriptores en el mismo proceso (ADR-006).</summary>
public interface IPublicadorEventos
{
    /// <summary>
    /// Entrega el evento a todos sus manejadores, en orden de registro. El llamador lo invoca
    /// DESPUÉS de su commit: si se publicara antes y la transacción fallara, los suscriptores
    /// reaccionarían a un cambio que nunca existió.
    /// </summary>
    /// <remarks>
    /// La implementación en memoria registra fallas ordinarias y continúa con el siguiente manejador.
    /// Una cancelación interrumpe la entrega. No hay persistencia ni reintento de eventos en este puerto.
    /// </remarks>
    /// <typeparam name="TEvento">Tipo de evento.</typeparam>
    /// <param name="evento">Hecho a publicar.</param>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Tarea que termina cuando todos los manejadores terminaron.</returns>
    Task PublicarAsync<TEvento>(TEvento evento, CancellationToken ct)
        where TEvento : IEventoDominio;
}
