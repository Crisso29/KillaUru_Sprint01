namespace KillaUru.Ingesta.Application.Puertos;

/// <summary>Puerto del barredor: descubrimiento de lotes pendientes y diagnóstico de salud (T-ARQ08).</summary>
internal interface IBarridoPendientes
{
    /// <summary>
    /// Devuelve hasta <paramref name="maximo"/> loteIds distintos (o null para filas sin lote_id)
    /// con estado Recibido, paginando por <paramref name="desde"/> (null = desde el inicio).
    /// </summary>
    Task<IReadOnlyList<Guid?>> LotesPendientesAsync(Guid? desde, int maximo, CancellationToken ct);

    /// <summary>Devuelve el recibido_utc de la fila Recibido más antigua, o null si no hay ninguna.</summary>
    Task<DateTimeOffset?> MasAntiguoPendienteAsync(CancellationToken ct);
}
