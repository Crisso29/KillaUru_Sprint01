using System.Data.Common;

namespace KillaUru.Shared.Persistencia;

/// <summary>
/// Transacción en curso de la petición o del trabajo (scoped). Permite que la auditoría se escriba
/// en la MISMA transacción que la acción (ADR-013) sin compartir el DbContext de otro módulo.
/// </summary>
public interface IContextoTransaccion
{
    /// <summary>Conexión de la transacción unida, o <c>null</c> si no hay ninguna.</summary>
    DbConnection? Conexion { get; }

    /// <summary>Transacción unida, o <c>null</c> si no hay ninguna.</summary>
    DbTransaction? Transaccion { get; }

    /// <summary>La llama el módulo que abre la transacción. Unir dos veces sin liberar es un error.</summary>
    /// <param name="conexion">Conexión abierta.</param>
    /// <param name="transaccion">Transacción iniciada sobre esa conexión.</param>
    void Unir(DbConnection conexion, DbTransaction transaccion);

    /// <summary>Agenda una acción para después del commit; se descarta si el rollback termina correctamente.</summary>
    /// <remarks>
    /// Las acciones se ejecutan en orden de registro. Si una falla, se propaga su excepción y las restantes
    /// no se ejecutan; el commit ya ocurrió y no se puede deshacer desde este contexto.
    /// </remarks>
    /// <param name="accion">Acción a ejecutar tras confirmar (p. ej. publicar un evento).</param>
    void AlConfirmar(Func<Task> accion);

    /// <summary>Confirma, limpia las referencias compartidas y ejecuta las acciones de <see cref="AlConfirmar"/>.</summary>
    /// <remarks>
    /// El módulo propietario conserva la responsabilidad de desechar la conexión y la transacción.
    /// Si el commit falla, el contexto conserva sus referencias y no ejecuta las acciones.
    /// </remarks>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Tarea del commit y de las acciones.</returns>
    Task ConfirmarAsync(CancellationToken ct);

    /// <summary>Revierte y, si termina correctamente, descarta las acciones y limpia las referencias compartidas.</summary>
    /// <remarks>No cierra ni desecha los recursos del módulo propietario.</remarks>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Tarea del rollback.</returns>
    Task RevertirAsync(CancellationToken ct);
}
