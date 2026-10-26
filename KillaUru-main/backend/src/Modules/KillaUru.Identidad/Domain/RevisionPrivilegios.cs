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

    /// <summary>Sin revisión previa vence ya; si no, el día 1 del mes siguiente en hora de Lima.</summary>
    internal static DateTimeOffset Vence(DateTimeOffset? ultima, DateTimeOffset ahora)
    {
        if (ultima is null) return ahora;
        var enLima = ultima.Value.ToOffset(HorasGuardadas.DesfaseLima);
        var inicioDelMes = new DateTimeOffset(enLima.Year, enLima.Month, 1, 0, 0, 0, HorasGuardadas.DesfaseLima);
        return inicioDelMes.AddMonths(1).ToUniversalTime();
    }
}
