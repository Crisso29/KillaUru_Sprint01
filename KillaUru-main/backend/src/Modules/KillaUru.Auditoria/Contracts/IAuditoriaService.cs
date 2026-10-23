namespace KillaUru.Auditoria.Contracts;

/// <summary>Única entrada entre módulos para dejar rastro de una acción sensible (ADR-013).</summary>
public interface IAuditoriaService
{
    /// <summary>Inserta en la transacción del llamador unida a IContextoTransaccion; no confirma.</summary>
    /// <remarks>El llamador debe revertir toda su operación ante un error. Sin transacción lanza
    /// InvalidOperationException. La métrica de riesgo se agenda con AlConfirmar y no sale al revertir.</remarks>
    /// <param name="accion">Código estable del catálogo.</param>
    /// <param name="actor">Id interno y rol, o Sistema.</param>
    /// <param name="objetivo">Recurso afectado, máximo 100 caracteres; nunca datos personales.</param>
    /// <param name="contexto">Datos no personales: se rechazan correo, email, contrasena, password
    /// y seudonimo, incluso anidados y sin distinguir mayúsculas.</param>
    /// <param name="ct">Cancelación de la escritura.</param>
    /// <returns>Tarea de inserción; el propietario confirma o revierte después.</returns>
    Task RegistrarAsync(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, CancellationToken ct);

    /// <summary>Confirma una lectura sensible en una transacción propia antes de devolver los datos.</summary>
    /// <remarks>Solo accesos administrativos a datos y exportaciones. Mapa y Dashboard no se auditan.
    /// Las métricas de riesgo se emiten después de confirmar.</remarks>
    /// <param name="accion">AccesoAdministrativoDatos o DatasetExportado.</param>
    /// <param name="actor">Id interno y rol, o Sistema.</param>
    /// <param name="objetivo">Recurso consultado.</param>
    /// <param name="contexto">Filtros o conteos, sujetos a la misma validación de privacidad.</param>
    /// <param name="ct">Cancelación de la operación.</param>
    /// <returns>Tarea de inserción y confirmación.</returns>
    Task RegistrarLecturaAsync(AccionAuditada accion, ActorAuditoria actor, string? objetivo,
        IReadOnlyDictionary<string, object?> contexto, CancellationToken ct);
}
