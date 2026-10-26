namespace KillaUru.Anomalias.Domain;

/// <summary>RN-11 y RN-13 usan recepción original incluso al recuperar trabajo atrasado.</summary>
internal sealed class ReglasCalidad(AnomaliasOptions opciones, CorredorVia corredor)
{
    /// <summary>Devuelve el primer motivo de descarte o null si el evento es aceptable (FR-001, FR-002).</summary>
    internal MotivoDescarte? EvaluarIntrinseca(ReporteAnomalia reporte, DateTimeOffset recibidoUtc)
    {
        if (reporte.Confidence.Valor < opciones.ConfidenceMinimo) return MotivoDescarte.ConfidenceBajo;
        if (reporte.PrecisionMetros > opciones.PrecisionGpsMaximaMetros) return MotivoDescarte.PrecisionGpsInsuficiente;
        var demasiadoAntiguo = reporte.DetectadoUtc < recibidoUtc.AddDays(-opciones.AntiguedadMaximaDias);
        // Se toleran unos minutos en el futuro: el reloj del celular puede ir adelantado.
        var enElFuturo = reporte.DetectadoUtc > recibidoUtc.AddMinutes(opciones.ToleranciaFuturoMinutos);
        if (demasiadoAntiguo || enElFuturo) return MotivoDescarte.TimestampFueraDeRango;
        // Redondeo a micrómetros para que el error numérico no convierta el borde inclusivo en rechazo.
        var distanciaVia = Math.Round(corredor.DistanciaMetros(reporte.Posicion), 6);
        return distanciaVia > opciones.DistanciaMaximaViaMetros ? MotivoDescarte.FueraDelCorredor : null;
    }
}
