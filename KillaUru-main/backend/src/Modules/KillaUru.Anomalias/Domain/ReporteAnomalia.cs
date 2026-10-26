using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Domain;

/// <summary>Reporte validado; el seudónimo puede purgarse sin alterar sus datos físicos.</summary>
internal sealed record ReporteAnomalia(Guid EventoId, TipoAnomalia Tipo, DateTimeOffset DetectadoUtc,
    DateTimeOffset RecibidoUtc, Coordenada Posicion, double PrecisionMetros, Severidad Severidad,
    Confidence Confidence, string ModeloVersion, Guid? Seudonimo, bool ConSesion, float KmVia = 0)
{
    public Guid AnomaliaId { get; init; }
    private ReporteAnomalia() : this(Guid.Empty, default, default, default, default, 0, default, default, "", null, false) { }
    internal static (ReporteAnomalia? Reporte, MotivoDescarte? Motivo) Desde(ReporteEntrada e, ContextoDeteccion c)
    {
        if (e.EventoId == Guid.Empty || e.DetectadoUtc is null || e.Latitud is null || e.Longitud is null
            || e.PrecisionMetros is null || !double.IsFinite(e.PrecisionMetros.Value) || e.PrecisionMetros < 0
            || e.Severidad is null || e.Confidence is null || e.Seudonimo is null || e.Seudonimo == Guid.Empty
            || string.IsNullOrWhiteSpace(e.ModeloVersion) || string.IsNullOrWhiteSpace(e.Tipo))
        { return (null, MotivoDescarte.EsquemaInvalido); }
        var punto = Coordenada.Crear(e.Latitud.Value, e.Longitud.Value);
        var severidad = Severidad.Crear(e.Severidad.Value);
        var confianza = Confidence.Crear(e.Confidence.Value);
        if (punto is null || severidad is null || confianza is null) return (null, MotivoDescarte.EsquemaInvalido);
        if (e.Tipo is not ("Bache" or "Hundimiento")) return (null, MotivoDescarte.TipoDesconocido);
        return (new(e.EventoId, Enum.Parse<TipoAnomalia>(e.Tipo), e.DetectadoUtc.Value.ToUniversalTime(),
            c.RecibidoUtc.ToUniversalTime(), punto.Value, e.PrecisionMetros.Value, severidad.Value,
            confianza.Value, e.ModeloVersion, e.Seudonimo, c.ConSesion), null);
    }
}
