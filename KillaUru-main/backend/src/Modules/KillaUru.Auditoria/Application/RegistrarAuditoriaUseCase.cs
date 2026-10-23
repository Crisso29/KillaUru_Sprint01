using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
using KillaUru.Shared.Persistencia;
namespace KillaUru.Auditoria.Application;

/// <summary>Une el rastro a la acción: el propietario de la transacción decide su resultado.</summary>
internal sealed class RegistrarAuditoriaUseCase(IRegistroAuditoriaRepositorio repositorio,
    IContextoTransaccion transaccion, IMetricas metricas, TimeProvider reloj)
{
    internal async Task EjecutarAsync(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, CancellationToken ct)
    {
        if (transaccion.Transaccion is null || transaccion.Conexion is null)
            throw new InvalidOperationException("Auditar una escritura exige la transacción del llamador.");
        var registro = CrearRegistro.De(accion, actor, objetivo, contexto, reloj);
        await repositorio.AgregarEnTransaccionAsync(registro, ct);
        if (CatalogoAcciones.EsDeRiesgo(registro.Accion))
            transaccion.AlConfirmar(() => { metricas.RegistrarAccionRiesgo(accion.ToString()); return Task.CompletedTask; });
    }
}
