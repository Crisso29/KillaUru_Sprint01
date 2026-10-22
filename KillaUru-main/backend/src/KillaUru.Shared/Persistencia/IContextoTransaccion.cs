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

    /// <summary>Agenda una acción para DESPUÉS del commit; se descarta si hay rollback.</summary>
    /// <param name="accion">Acción a ejecutar tras confirmar (p. ej. publicar un evento).</param>
    void AlConfirmar(Func<Task> accion);

    /// <summary>Commit, luego las acciones de <see cref="AlConfirmar"/>, y libera la transacción.</summary>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Tarea del commit y de las acciones.</returns>
    Task ConfirmarAsync(CancellationToken ct);

    /// <summary>Rollback, descarta las acciones pendientes y libera la transacción.</summary>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Tarea del rollback.</returns>
    Task RevertirAsync(CancellationToken ct);
}
