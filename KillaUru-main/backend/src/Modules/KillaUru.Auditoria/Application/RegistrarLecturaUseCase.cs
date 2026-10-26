using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
namespace KillaUru.Auditoria.Application;

/// <summary>Una lectura no trae transacción: confirma su rastro antes de entregar información sensible.</summary>
internal sealed class RegistrarLecturaUseCase(IRegistroAuditoriaRepositorio repositorio, IMetricas metricas, TimeProvider reloj)
{
    internal async Task EjecutarAsync(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, CancellationToken ct)
    {
        // Solo dos acciones de lectura son suficientemente sensibles para auditarse con transacción propia.
        var esLecturaSensible = accion is AccionAuditada.AccesoAdministrativoDatos or AccionAuditada.DatasetExportado;
        if (!esLecturaSensible)
            throw new InvalidOperationException("Esta operación solo registra lecturas sensibles.");
        var registro = CrearRegistro.De(accion, actor, objetivo, contexto, reloj);
        await repositorio.AgregarConTransaccionPropiaAsync(registro, ct);
        if (CatalogoAcciones.EsDeRiesgo(registro.Accion))
            metricas.RegistrarAccionRiesgo(accion.ToString());
    }
}
