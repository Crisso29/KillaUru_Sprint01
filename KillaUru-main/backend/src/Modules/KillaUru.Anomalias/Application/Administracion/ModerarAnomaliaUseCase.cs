using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Contracts;
using KillaUru.Anomalias.Domain;
using KillaUru.Auditoria.Contracts;
using KillaUru.Shared.Eventos;
using Microsoft.Extensions.Options;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Revalida con bloqueo y une la auditoría a la misma transacción de la decisión.</summary>
internal sealed class ModerarAnomaliaUseCase(IAnomaliaRepository repo, IDetalleAnomaliaRepository detalles,
    ITransaccionAnomalias tx, IAuditoriaService auditoria, IPublicadorEventos eventos, TimeProvider reloj, IOptions<AnomaliasOptions> opciones)
{
    internal async Task<ResultadoModeracion> EjecutarAsync(Guid id, ComandoModeracion comando, ActorAnomalias actor, CancellationToken ct)
    {
        if (comando.Accion is null || !Enum.IsDefined(comando.Accion.Value)) return new(null, "datos_invalidos");
        var previa = await repo.ObtenerAsync(id, ct);
        if (previa is null) return new(null, "no_encontrado");
        return await tx.EjecutarAsync([previa.Celda.Id], null, null, () => AplicarAsync(id, comando.Accion.Value, actor, ct), ct);
    }
    private async Task<ResultadoModeracion> AplicarAsync(Guid id, AccionModeracion accion, ActorAnomalias actor, CancellationToken ct)
    {
        var anomalia = await repo.ObtenerParaActualizarAsync(id, ct);
        if (anomalia is null) return new(null, "no_encontrado");
        var antes = anomalia.Estado.ToString();
        if (!anomalia.Moderar(Mapear(accion), reloj.GetUtcNow(), opciones.Value, actor.Id)) return new(null, "transicion_no_permitida");
        await repo.GuardarAsync(anomalia, false, ct);
        await auditoria.RegistrarAsync(AccionAuditada.AnomaliaModerada, new(actor.Id, actor.Rol), id.ToString(),
            new Dictionary<string, object?> { ["accion"] = accion.ToString(), ["desde"] = antes, ["hacia"] = anomalia.Estado.ToString() }, ct);
        tx.AlConfirmar(() => eventos.PublicarAsync(new AnomaliaCambiada(id, anomalia.Celda), CancellationToken.None));
        return new(await detalles.ObtenerAsync(id, ct));
    }
    private static AccionAdmin Mapear(AccionModeracion accion) => accion switch
    {
        AccionModeracion.MarcarReparada => AccionAdmin.MarcarReparada, AccionModeracion.Descartar => AccionAdmin.Descartar,
        AccionModeracion.Reabrir => AccionAdmin.Reabrir, AccionModeracion.MantenerDescartada => AccionAdmin.MantenerDescartada,
        _ => throw new ArgumentOutOfRangeException(nameof(accion)),
    };
}
