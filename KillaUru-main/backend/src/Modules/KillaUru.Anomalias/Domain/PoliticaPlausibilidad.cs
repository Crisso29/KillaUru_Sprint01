namespace KillaUru.Anomalias.Domain;

/// <summary>RN-15 aplica el tope a detecciones y descuenta incertidumbre GPS antes de estimar velocidad.</summary>
internal sealed class PoliticaPlausibilidad(AnomaliasOptions opciones)
{
    private const double MetrosPorSegundoAKmh = 3.6;

    internal bool SuperaTope(int eventos) => eventos > opciones.MaxEventosPorHoraDeteccion;

    /// <summary>
    /// Velocidad imposible entre dos eventos del mismo seudónimo. Con menos de 10 s no se evalúa:
    /// el error del GPS dominaría la estimación (FR-004).
    /// </summary>
    internal MotivoDescarte? Evaluar(ReporteAnomalia actual, ReporteAnomalia? anterior)
    {
        if (anterior is null || anterior.Seudonimo != actual.Seudonimo) return null;
        var segundos = (actual.DetectadoUtc - anterior.DetectadoUtc).TotalSeconds;
        if (segundos < opciones.SeparacionMinimaPlausibilidadSegundos) return null;
        // Lo que ambos GPS podrían haberse equivocado se resta: solo cuenta la distancia segura.
        var errorGps = actual.PrecisionMetros + anterior.PrecisionMetros;
        var distanciaSegura = Math.Max(0, GeodesiaLocal.Distancia(actual.Posicion, anterior.Posicion) - errorGps);
        // Redondeo a 6 decimales para que el error numérico no convierta 150 km/h exactos en 150,0000001.
        var kmh = Math.Round(distanciaSegura / segundos * MetrosPorSegundoAKmh, 6);
        return kmh > opciones.VelocidadMaximaPlausibleKmh ? MotivoDescarte.VelocidadImposible : null;
    }
}
