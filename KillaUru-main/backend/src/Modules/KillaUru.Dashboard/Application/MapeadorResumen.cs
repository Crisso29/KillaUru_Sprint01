using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Application;

/// <summary>FR-007…FR-013: convierte los conteos en el resumen y su serie comparativa.</summary>
internal static class MapeadorResumen
{
    internal static ResumenDashboard Crear(IReadOnlyList<DiaIndicadores?> datos, DateOnly desde,
        int dias, DateTimeOffset? calculadoUtc, string urlPanelesTecnicos)
    {
        var dIdx    = IndiceDiaActual(datos);
        var trimmed = datos.Take(dIdx + 1).ToList();
        var slice   = trimmed.Count >= 14 ? trimmed.GetRange(trimmed.Count - 14, 14) : trimmed;

        return new(
            dIdx >= 0 ? desde.AddDays(dIdx) : null,
            calculadoUtc,
            IndicadorResumen.Desde(CalculadoraIndicadores.CalcularEventos(slice),      "eventos"),
            IndicadorResumen.Desde(CalculadoraIndicadores.CalcularConductores(slice),   "cuentas"),
            IndicadorResumen.Desde(CalculadoraIndicadores.CalcularDispositivos(slice),  "dispositivos"),
            IndicadorResumen.Desde(CalculadoraIndicadores.CalcularCobertura(slice),     "%"),
            IndicadorResumen.Desde(CalculadoraIndicadores.CalcularConfirmacion(slice),  "%"),
            IndicadorResumen.Desde(CalculadoraIndicadores.CalcularReaparicion(slice),   "%"),
            CalcularEventosPorDia(trimmed, dias, dIdx, desde),
            urlPanelesTecnicos);
    }

    private static int IndiceDiaActual(IReadOnlyList<DiaIndicadores?> datos)
    {
        for (int i = datos.Count - 1; i >= 0; i--)
            if (datos[i] is not null) return i;
        return -1;
    }

    private static EventoPorDia[] CalcularEventosPorDia(
        List<DiaIndicadores?> datos, int dias, int dIdx, DateOnly desde)
    {
        if (dIdx < 0) return [];
        var ultDia    = desde.AddDays(dIdx);
        var resultado = new EventoPorDia[dias];
        for (int i = 0; i < dias; i++)
        {
            var diaCurrent  = ultDia.AddDays(-(dias - 1) + i);
            int idxCurrent  = dIdx - (dias - 1) + i;
            int idxPrevious = idxCurrent - dias;
            var ev    = idxCurrent  >= 0 && idxCurrent  < datos.Count ? datos[idxCurrent]?.EventosRecibidos  : null;
            var evAnt = idxPrevious >= 0 && idxPrevious < datos.Count ? datos[idxPrevious]?.EventosRecibidos : null;
            resultado[i] = new(diaCurrent, ev, evAnt);
        }
        return resultado;
    }
}

