using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Escritura del agregado; las lecturas de modificación requieren transacción y bloqueo de fila.</summary>
internal interface IAnomaliaRepository
{
    Task<Anomalia?> MasCercanaAsync(Coordenada punto, TipoAnomalia tipo, double radio, CancellationToken ct);
    Task<Anomalia?> ObtenerParaActualizarAsync(Guid id, CancellationToken ct);
    Task<Anomalia?> ObtenerAsync(Guid id, CancellationToken ct);
    Task GuardarAsync(Anomalia anomalia, bool nueva, CancellationToken ct);
}
