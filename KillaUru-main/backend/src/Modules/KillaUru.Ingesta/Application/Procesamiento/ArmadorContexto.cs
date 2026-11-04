using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Ingesta.Domain;

namespace KillaUru.Ingesta.Application.Procesamiento;

/// <summary>
/// Construye el ContextoReporte para un evento pendiente: ConSesion del evento,
/// conteo de eventos del seudónimo en su hora de detección (RN-15a) y RecibidoUtc del lote.
/// </summary>
internal sealed class ArmadorContexto(IPendientesRepositorio repositorio)
{
    internal async Task<ContextoReporte> ArmarAsync(EventoPendiente pendiente, CancellationToken ct)
    {
        var seudonimo = pendiente.Evento.Seudonimo;
        int eventosEnHora = 0;
        if (seudonimo.HasValue && pendiente.Evento.DetectadoUtc.HasValue)
        {
            var (inicio, _) = HoraDeDeteccion.Bloque(pendiente.Evento.DetectadoUtc.Value);
            eventosEnHora = await repositorio.ContarEnLaHoraAsync(seudonimo.Value, inicio, ct);
        }
        return new ContextoReporte(pendiente.ConSesion, eventosEnHora, pendiente.RecibidoUtc);
    }
}
