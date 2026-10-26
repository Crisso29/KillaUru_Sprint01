using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Auditoria.Contracts;
using KillaUru.Shared.Eventos;
using Microsoft.Extensions.Options;

namespace KillaUru.Anomalias.Application.Administracion;

/// <summary>Revalida con bloqueo y une la auditoría a la misma transacción de la decisión.</summary>
internal sealed class ModerarAnomaliaUseCase(IAnomaliaRepository anomalias, IDetalleAnomaliaRepository detalles,
    ITransaccionAnomalias transaccion, IAuditoriaService auditoria, IPublicadorEventos eventos, TimeProvider reloj,
    IOptions<AnomaliasOptions> opciones)
{
    internal async Task<ResultadoModeracion> EjecutarAsync(Guid id, ComandoModeracion comando, ActorAnomalias actor, CancellationToken ct)
    {
        if (comando.Accion is null || !Enum.IsDefined(comando.Accion.Value)) return new(null, "datos_invalidos");
        // Primera lectura sin bloqueo, solo para saber qué celda bloquear.
        var previa = await anomalias.ObtenerAsync(id, ct);
        if (previa is null) return new(null, "no_encontrado");
        return await transaccion.EjecutarAsync([previa.Celda.Id], null, null, () => AplicarAsync(id, comando.Accion.Value, actor, ct), ct);
    }

    /// <summary>
    /// Se vuelve a leer con FOR UPDATE dentro del bloqueo: el procesamiento pudo cambiar el estado
    /// entre ambas lecturas (FR-030).
    /// </summary>
    private async Task<ResultadoModeracion> AplicarAsync(Guid id, AccionModeracion accion, ActorAnomalias actor, CancellationToken ct)
    {
        var anomalia = await anomalias.ObtenerParaActualizarAsync(id, ct);
        if (anomalia is null) return new(null, "no_encontrado");
        var estadoAnterior = anomalia.Estado.ToString();
        if (!anomalia.Moderar(Mapear(accion), reloj.GetUtcNow(), opciones.Value, actor.Id)) return new(null, "transicion_no_permitida");
        await anomalias.GuardarAsync(anomalia, false, ct);
        var contexto = new Dictionary<string, object?>
        {
            ["accion"] = accion.ToString(), ["desde"] = estadoAnterior, ["hacia"] = anomalia.Estado.ToString(),
        };
        await auditoria.RegistrarAsync(AccionAuditada.AnomaliaModerada, new(actor.Id, actor.Rol), id.ToString(), contexto, ct);
        transaccion.AlConfirmar(() => eventos.PublicarAsync(new AnomaliaCambiada(id, anomalia.Celda), CancellationToken.None));
        return new(await detalles.ObtenerAsync(id, ct));
    }

    /// <summary>La acción que llega por la API se traduce a la del dominio, que no conoce el formato público.</summary>
    private static AccionAdmin Mapear(AccionModeracion accion) => accion switch
    {
        AccionModeracion.MarcarReparada => AccionAdmin.MarcarReparada,
        AccionModeracion.Descartar => AccionAdmin.Descartar,
        AccionModeracion.Reabrir => AccionAdmin.Reabrir,
        AccionModeracion.MantenerDescartada => AccionAdmin.MantenerDescartada,
        _ => throw new ArgumentOutOfRangeException(nameof(accion)),
    };
}
