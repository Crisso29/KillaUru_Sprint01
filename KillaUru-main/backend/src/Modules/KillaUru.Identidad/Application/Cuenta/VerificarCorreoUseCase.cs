using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Canjea exclusivamente enlaces de verificación aún vigentes.</summary>
internal sealed class VerificarCorreoUseCase(ResolverEnlace resolver, ITransaccionIdentidad transaccion, TimeProvider reloj)
{
    internal Task<ResultadoGestion> EjecutarAsync(string token, CancellationToken ct) => transaccion.EjecutarAsync(async () =>
    {
        var (usuario, enlace) = await resolver.CargarAsync(token, ct);
        if (enlace is null || enlace.Proposito != PropositoToken.Verificacion || !enlace.PuedeUsarse(reloj.GetUtcNow()))
            return new ResultadoGestion("enlace_invalido", 400);
        if (usuario is null) return new ResultadoGestion("enlace_invalido", 400);
        enlace.Usar(reloj.GetUtcNow());
        usuario.MarcarCorreoVerificado();
        return new ResultadoGestion();
    }, ct);
}
