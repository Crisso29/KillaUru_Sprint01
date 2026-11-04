using KillaUru.Anomalias.Contracts.Lecturas;
using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Application;

internal static class MapeoResumenDia
{
    internal static DiaIndicadores Desde(ResumenDia r, long conductoresActivos) =>
        new(r.Dia, r.EventosRecibidos, conductoresActivos, r.DispositivosDistintos,
            r.TramosConReportes7d, r.TramosTotales, r.Confirmaciones,
            r.PendientesExpiradas, r.Expiraciones, r.Renacimientos);
}
