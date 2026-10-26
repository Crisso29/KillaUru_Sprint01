namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Límite atómico que une la auditoría; los efectos externos se ejecutan después del commit.</summary>
internal interface ITransaccionIdentidad
{
    Task<T> EjecutarAsync<T>(Func<Task<T>> accion, CancellationToken ct, bool protegerAdministradores = false);
    void AlConfirmar(Func<Task> accion);
}
