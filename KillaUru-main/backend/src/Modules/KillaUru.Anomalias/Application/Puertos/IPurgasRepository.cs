namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Retención por lotes: no carga identidades ni reportes en memoria.</summary>
internal interface IPurgasRepository
{
    Task<int> PurgarSeudonimosAsync(DateTimeOffset limite, CancellationToken ct);
    Task<int> PurgarHistoricoAsync(DateTimeOffset limite, CancellationToken ct);
}
