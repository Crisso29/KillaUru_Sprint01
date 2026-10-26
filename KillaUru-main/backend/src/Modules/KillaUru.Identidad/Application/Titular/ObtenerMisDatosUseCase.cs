using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;

namespace KillaUru.Identidad.Application.Titular;

/// <summary>Solo obtiene los datos del sub autenticado, sin aceptar ids externos.</summary>
internal sealed class ObtenerMisDatosUseCase(IUsuarioRepositorio usuarios)
{
    internal async Task<MisDatos?> EjecutarAsync(Guid id, CancellationToken ct)
    {
        var usuario = await usuarios.PorIdAsync(id, ct);
        if (usuario is null || !usuario.Activo) return null;
        var aceptaciones = await usuarios.AceptacionesAsync(id, ct);
        return new(usuario.Correo.Valor, usuario.Rol.ToString(), usuario.CreadoUtc, usuario.CorreoVerificado,
            aceptaciones.Select(x => new AceptacionVista(x.Documento.ToString(), x.Version, x.AceptadoUtc)).ToArray());
    }
}
