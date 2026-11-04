namespace KillaUru.Dashboard.Application;

internal sealed record EventoPorDia(DateOnly Dia, long? Eventos, long? EventosAnterior);
