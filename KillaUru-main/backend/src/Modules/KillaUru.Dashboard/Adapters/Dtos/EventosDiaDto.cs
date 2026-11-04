using KillaUru.Dashboard.Application;

namespace KillaUru.Dashboard.Adapters.Dtos;

/// <summary>Eventos de un día comparado con el día equivalente del periodo anterior.</summary>
internal sealed record EventosDiaDto(DateOnly Dia, long? Eventos, long? EventosAnterior)
{
    internal static EventosDiaDto Desde(EventoPorDia e) => new(e.Dia, e.Eventos, e.EventosAnterior);
}
