using KillaUru.Anomalias.Application.Puertos;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Consulta administrativa ListarTransicionesRecientes; no modifica anomalías.</summary>
internal sealed class ListarTransicionesRecientesUseCase(ILecturaAdministracionRepository repo)
{
    internal Task<IReadOnlyList<TransicionReciente>> EjecutarAsync(int limite, CancellationToken ct) => repo.TransicionesAsync(Math.Clamp(limite, 1, 50), ct);
}
