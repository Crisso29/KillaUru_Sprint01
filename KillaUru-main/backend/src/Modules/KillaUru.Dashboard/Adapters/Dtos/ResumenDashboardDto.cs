using KillaUru.Dashboard.Application;

namespace KillaUru.Dashboard.Adapters.Dtos;

/// <summary>Respuesta del endpoint GET /api/v1/dashboard/indicadores.</summary>
internal sealed record ResumenDashboardDto(
    DateOnly?            UltimoDiaCerrado,
    DateTimeOffset?      CalculadoUtc,
    IndicadorDto         EventosDia,
    IndicadorDto         ConductoresActivos,
    IndicadorDto         DispositivosDia,
    IndicadorDto         CoberturaPct,
    IndicadorDto         ConfirmacionPct,
    IndicadorDto         ReaparicionPct,
    IReadOnlyList<EventosDiaDto> EventosPorDia,
    string               UrlPanelesTecnicos)
{
    internal static ResumenDashboardDto Desde(ResumenDashboard r) =>
        new(r.UltimoDiaCerrado, r.CalculadoUtc,
            IndicadorDto.Desde(r.EventosDia),
            IndicadorDto.Desde(r.ConductoresActivos),
            IndicadorDto.Desde(r.DispositivosDia),
            IndicadorDto.Desde(r.CoberturaPct),
            IndicadorDto.Desde(r.ConfirmacionPct),
            IndicadorDto.Desde(r.ReaparicionPct),
            r.EventosPorDia.Select(EventosDiaDto.Desde).ToList(),
            r.UrlPanelesTecnicos);
}
