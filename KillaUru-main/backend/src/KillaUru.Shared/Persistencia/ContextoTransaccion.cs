using System.Data.Common;

namespace KillaUru.Shared.Persistencia;

/// <summary>
/// Implementación scoped de <see cref="IContextoTransaccion"/>. No es dueña de la conexión ni de la
/// transacción (las abre y las desecha el módulo que actúa): solo las comparte y ordena el
/// «después del commit».
/// </summary>
public sealed class ContextoTransaccion : IContextoTransaccion
{
    private readonly List<Func<Task>> _alConfirmar = [];

    /// <inheritdoc />
    public DbConnection? Conexion { get; private set; }

    /// <inheritdoc />
    public DbTransaction? Transaccion { get; private set; }

    /// <inheritdoc />
    public void Unir(DbConnection conexion, DbTransaction transaccion)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        ArgumentNullException.ThrowIfNull(transaccion);
        if (Transaccion is not null)
        {
            throw new InvalidOperationException("Ya hay una transacción unida: confírmala o reviértela antes de unir otra.");
        }

        Conexion = conexion;
        Transaccion = transaccion;
    }

    /// <inheritdoc />
    public void AlConfirmar(Func<Task> accion)
    {
        ArgumentNullException.ThrowIfNull(accion);
        RequerirTransaccion();
        _alConfirmar.Add(accion);
    }

    /// <inheritdoc />
    public async Task ConfirmarAsync(CancellationToken ct)
    {
        await RequerirTransaccion().CommitAsync(ct).ConfigureAwait(false);
        var acciones = _alConfirmar.ToArray();
        // La transacción ya terminó: los callbacks no deben reutilizarla para nuevas escrituras.
        Liberar();
        foreach (var accion in acciones)
        {
            await accion().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task RevertirAsync(CancellationToken ct)
    {
        await RequerirTransaccion().RollbackAsync(ct).ConfigureAwait(false);
        Liberar();
    }

    private DbTransaction RequerirTransaccion() =>
        Transaccion ?? throw new InvalidOperationException("No hay una transacción unida.");

    // Solo limpia el contexto scoped; el módulo que abrió los recursos ejecuta DisposeAsync.
    private void Liberar()
    {
        Conexion = null;
        Transaccion = null;
        _alConfirmar.Clear();
    }
}
