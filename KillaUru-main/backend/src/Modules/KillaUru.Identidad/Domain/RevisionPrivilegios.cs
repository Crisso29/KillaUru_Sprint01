namespace KillaUru.Identidad.Domain;

/// <summary>Constancia mensual agregada; los ids revisados quedan en Auditoría.</summary>
internal sealed class RevisionPrivilegios
{
    public long Id { get; private set; }
    public DateTimeOffset RealizadaUtc { get; private set; }
    public Guid ActorId { get; private set; }
    public short Confirmados { get; private set; }
    public short Retirados { get; private set; }
    internal static RevisionPrivilegios Crear(Guid actor, short confirmados, short retirados, DateTimeOffset ahora) =>
        new() { ActorId = actor, Confirmados = confirmados, Retirados = retirados, RealizadaUtc = ahora };

    internal static DateTimeOffset Vence(DateTimeOffset? ultima, DateTimeOffset ahora)
    {
        if (ultima is null) return ahora;
        var local = ultima.Value.ToOffset(TimeSpan.FromHours(-5));
        return new DateTimeOffset(local.Year, local.Month, 1, 0, 0, 0, local.Offset).AddMonths(1).ToUniversalTime();
    }
}
