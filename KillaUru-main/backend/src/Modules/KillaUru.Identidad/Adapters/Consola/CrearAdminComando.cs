using KillaUru.Identidad.Application.Usuarios;
namespace KillaUru.Identidad.Adapters.Consola;

/// <summary>Única salida autorizada del enlace inicial; el llamador usa un contenedor efímero sin logs.</summary>
internal sealed class CrearAdminComando(CrearAdministradorInicialUseCase crear)
{
    internal async Task<int> EjecutarAsync(string correo, TextWriter salida, CancellationToken ct)
    {
        var resultado = await crear.EjecutarAsync(correo, ct);
        await salida.WriteLineAsync((resultado.Enlace ?? resultado.Error).AsMemory(), ct);
        return resultado.Error is null ? 0 : 1;
    }
}
