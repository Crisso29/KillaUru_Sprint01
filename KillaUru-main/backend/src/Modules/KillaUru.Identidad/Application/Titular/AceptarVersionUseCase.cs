using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Application.Titular;

/// <summary>Aceptación idempotente de versiones vigentes; nunca sobreescribe el historial.</summary>
internal sealed class AceptarVersionUseCase(IUsuarioRepositorio usuarios, ITransaccionIdentidad transaccion,
    IOptions<IdentidadOptions> opciones, TimeProvider reloj)
{
    internal Task<ResultadoGestion> EjecutarAsync(Guid id, string documento, string version, CancellationToken ct)
    {
        if (documento is not ("PoliticaPrivacidad" or "TerminosUso"))
            return Task.FromResult(new ResultadoGestion("datos_invalidos", 400));
        var vigente = documento == "PoliticaPrivacidad" ? opciones.Value.VersionPolitica : opciones.Value.VersionTerminos;
        if (version != vigente) return Task.FromResult(new ResultadoGestion("aceptacion_requerida", 400));
        return transaccion.EjecutarAsync(async () =>
        {
            var usuario = await usuarios.PorIdAsync(id, ct);
            if (usuario is null || !usuario.Activo) return new ResultadoGestion("token_invalido", 401);
            await usuarios.AceptarAsync(Consentimiento.Crear(id, Enum.Parse<DocumentoLegal>(documento), version, reloj.GetUtcNow()), ct);
            return new ResultadoGestion();
        }, ct);
    }
}
