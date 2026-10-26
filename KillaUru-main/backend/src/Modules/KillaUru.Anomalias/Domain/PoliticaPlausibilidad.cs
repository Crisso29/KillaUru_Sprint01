namespace KillaUru.Anomalias.Domain;

/// <summary>RN-15 aplica el tope a detecciones y descuenta incertidumbre GPS antes de estimar velocidad.</summary>
internal sealed class PoliticaPlausibilidad(AnomaliasOptions opciones)
{
    internal bool SuperaTope(int eventos) => eventos > opciones.MaxEventosPorHoraDeteccion;
    internal MotivoDescarte? Evaluar(ReporteAnomalia actual, ReporteAnomalia? anterior)
    {
        if (anterior is null || anterior.Seudonimo != actual.Seudonimo) return null;
        var segundos = (actual.DetectadoUtc - anterior.DetectadoUtc).TotalSeconds;
        if (segundos < opciones.SeparacionMinimaPlausibilidadSegundos) return null;
        var distancia = Math.Max(0, GeodesiaLocal.Distancia(actual.Posicion, anterior.Posicion)
            - actual.PrecisionMetros - anterior.PrecisionMetros);
        return Math.Round(distancia / segundos * 3.6, 6) > opciones.VelocidadMaximaPlausibleKmh
            ? MotivoDescarte.VelocidadImposible : null;
    }
}
