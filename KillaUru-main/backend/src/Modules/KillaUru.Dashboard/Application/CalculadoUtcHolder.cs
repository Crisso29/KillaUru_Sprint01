namespace KillaUru.Dashboard.Application;

internal sealed class CalculadoUtcHolder
{
    internal DateTimeOffset? UltimaCalculacion { get; private set; }
    internal void Actualizar(DateTimeOffset ahora) => UltimaCalculacion = ahora;
}
