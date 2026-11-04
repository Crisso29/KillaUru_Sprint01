namespace KillaUru.Dashboard.Application;

internal sealed record ResumenDashboard(
    DateOnly?                  UltimoDiaCerrado,
    DateTimeOffset?            CalculadoUtc,
    IndicadorResumen           EventosDia,
    IndicadorResumen           ConductoresActivos,
    IndicadorResumen           DispositivosDia,
    IndicadorResumen           CoberturaPct,
    IndicadorResumen           ConfirmacionPct,
    IndicadorResumen           ReaparicionPct,
    IReadOnlyList<EventoPorDia> EventosPorDia,
    string                     UrlPanelesTecnicos);
