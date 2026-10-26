using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Idempotencia e historial mínimo para reglas; no expone cuentas.</summary>
internal interface IReporteRepository
{
    Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct);
    Task<IReadOnlyList<ReporteAnomalia>> DesdeAsync(Guid anomalia, DateTimeOffset desde, CancellationToken ct);
    Task<ReporteAnomalia?> AnteriorAsync(Guid seudonimo, DateTimeOffset detectado, CancellationToken ct);
    void Agregar(ReporteAnomalia reporte, Guid anomalia);
}
