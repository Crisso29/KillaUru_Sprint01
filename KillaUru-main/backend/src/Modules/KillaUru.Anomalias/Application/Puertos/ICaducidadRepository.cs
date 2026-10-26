using KillaUru.Anomalias.Contracts;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Caduca un lote bloqueado sin esperar filas en procesamiento.</summary>
internal interface ICaducidadRepository
{
    Task<IReadOnlyList<AnomaliaCambiada>> CaducarAsync(DateTimeOffset ahora, CancellationToken ct);
}
