namespace KillaUru.Anomalias.Domain;
/// <summary>Cambio único por operación; actor nulo identifica una decisión automática.</summary>
internal sealed record Transicion(EstadoAnomalia? Desde, EstadoAnomalia Hacia, DateTimeOffset OcurridaUtc, Guid? ActorId)
{
    public long Id { get; init; }
    public Guid AnomaliaId { get; init; }
}
