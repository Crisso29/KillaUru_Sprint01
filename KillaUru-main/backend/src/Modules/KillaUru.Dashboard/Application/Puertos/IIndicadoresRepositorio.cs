using KillaUru.Dashboard.Domain;

namespace KillaUru.Dashboard.Application.Puertos;

internal interface IIndicadoresRepositorio
{
    Task<IReadOnlySet<DateOnly>>         DiasCompletosAsync(DateOnly desde, DateOnly hasta, CancellationToken ct);
    Task                                 GuardarDiaAsync(DiaIndicadores dia, CancellationToken ct);
    Task<IReadOnlyList<DiaIndicadores?>> LeerAsync(DateOnly desde, DateOnly hasta, CancellationToken ct);
    Task<int>                            PurgarAsync(DateOnly anteDe, CancellationToken ct);
}
