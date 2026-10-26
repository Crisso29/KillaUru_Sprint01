namespace KillaUru.Anomalias.Domain;

/// <summary>RN-11 y RN-13 usan recepción original incluso al recuperar trabajo atrasado.</summary>
internal sealed class ReglasCalidad(AnomaliasOptions opciones, CorredorVia corredor)
{
    internal MotivoDescarte? EvaluarIntrinseca(ReporteAnomalia reporte, DateTimeOffset recibidoUtc)
    {
        if (reporte.Confidence.Valor < opciones.ConfidenceMinimo) return MotivoDescarte.ConfidenceBajo;
        if (reporte.PrecisionMetros > opciones.PrecisionGpsMaximaMetros) return MotivoDescarte.PrecisionGpsInsuficiente;
        if (reporte.DetectadoUtc < recibidoUtc.AddDays(-opciones.AntiguedadMaximaDias)
            || reporte.DetectadoUtc > recibidoUtc.AddMinutes(opciones.ToleranciaFuturoMinutos))
        { return MotivoDescarte.TimestampFueraDeRango; }
        // Redondeo a micrómetros para que el error numérico no convierta el borde inclusivo en rechazo.
        return Math.Round(corredor.DistanciaMetros(reporte.Posicion), 6) > opciones.DistanciaMaximaViaMetros
            ? MotivoDescarte.FueraDelCorredor : null;
    }
}
