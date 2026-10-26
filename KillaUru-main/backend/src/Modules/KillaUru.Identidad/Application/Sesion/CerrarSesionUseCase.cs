using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;

namespace KillaUru.Identidad.Application.Sesion;

/// <summary>El jti firmado identifica la familia incluso cuando la cookie refresh no llega a esta ruta.</summary>
internal sealed class CerrarSesionUseCase(ISesionRepositorio sesiones, IUsuarioRepositorio usuarios, ITransaccionIdentidad transaccion, TimeProvider reloj)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid familia, CancellationToken ct) =>
        transaccion.EjecutarAsync(async () =>
        {
            var id = await sesiones.UsuarioPorFamiliaAsync(familia, ct);
            if (id is not null) await usuarios.PorIdAsync(id.Value, ct);
            await sesiones.RevocarFamiliaAsync(familia, reloj.GetUtcNow(), ct);
            return new ResultadoGestion();
        }, ct);
}
