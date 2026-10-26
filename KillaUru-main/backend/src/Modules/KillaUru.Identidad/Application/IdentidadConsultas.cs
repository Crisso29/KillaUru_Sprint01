using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Contracts;
namespace KillaUru.Identidad.Application;

/// <summary>Fachada agregada: nunca une identidad con eventos.</summary>
internal sealed class IdentidadConsultas(IUsuarioRepositorio usuarios, TimeProvider reloj) : IIdentidadConsultas
{
    public Task<int> ContarConductoresActivosAsync(int dias, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dias);
        return usuarios.ContarConductoresActivosAsync(reloj.GetUtcNow().AddDays(-dias), ct);
    }
}
