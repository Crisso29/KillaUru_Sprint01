using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KillaUru.Shared.Eventos;

/// <summary>
/// Publicador en memoria, sin broker (ADR-006). Resuelve los manejadores en un scope propio
/// para que puedan usar dependencias scoped, y aísla sus fallas: el evento ya ocurrió (el commit
/// está hecho), así que una excepción distinta de OperationCanceledException se registra y no impide
/// a los demás. Una cancelación se propaga; los manejadores restantes no reciben el evento.
/// </summary>
/// <param name="scopes">Fábrica de scopes del contenedor.</param>
/// <param name="logger">Registro de fallas de los manejadores.</param>
public sealed class PublicadorEnMemoria(IServiceScopeFactory scopes, ILogger<PublicadorEnMemoria> logger)
    : IPublicadorEventos
{
    private static readonly Action<ILogger, string, string, Exception?> FallaManejador =
        LoggerMessage.Define<string, string>(
            LogLevel.Error, new EventId(1, nameof(FallaManejador)), "El manejador {Manejador} falló con el evento {Evento}");

    /// <inheritdoc />
    public async Task PublicarAsync<TEvento>(TEvento evento, CancellationToken ct)
        where TEvento : IEventoDominio
    {
        await using var scope = scopes.CreateAsyncScope();
        foreach (var manejador in scope.ServiceProvider.GetServices<IManejador<TEvento>>())
        {
            try
            {
                await manejador.ManejarAsync(evento, ct).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Aislar al suscriptor es el objetivo: ninguna falla debe frenar a los demás.
            catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
            {
                FallaManejador(logger, manejador.GetType().Name, typeof(TEvento).Name, ex);
            }
        }
    }
}
