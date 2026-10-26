using KillaUru.Anomalias.Application.Puertos;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Consulta administrativa ListarTransicionesRecientes; no modifica anomalías.</summary>
internal sealed class ListarTransicionesRecientesUseCase(ILecturaAdministracionRepository repo)
{
    /// <summary>Máximo del contrato (`limite` 1–50 en anomalias.openapi.yaml); fuera de rango se ajusta en vez de fallar.</summary>
    private const int MaximoPorConsulta = 50;

    internal Task<IReadOnlyList<TransicionReciente>> EjecutarAsync(int limite, CancellationToken ct) =>
        repo.TransicionesAsync(Math.Clamp(limite, 1, MaximoPorConsulta), ct);
}
