namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Transacción del módulo con bloqueo ordenado y callbacks posteriores al commit.</summary>
internal interface ITransaccionAnomalias
{
    Task<T> EjecutarAsync<T>(IReadOnlyCollection<long> celdas, Guid? evento, Guid? seudonimo, Func<Task<T>> accion, CancellationToken ct);
    void AlConfirmar(Func<Task> accion);
}
