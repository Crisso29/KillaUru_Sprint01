namespace KillaUru.Anomalias.Application.Tareas;

/// <summary>
/// Filas por lote de las tareas de mantenimiento. Los repositorios lo usan como LIMIT y los casos de
/// uso repiten mientras un lote salga lleno: ambos deben leer este mismo valor o la tarea se cortaría
/// antes de terminar. Lotes acotados mantienen cortos los bloqueos de fila.
/// </summary>
internal static class LoteMantenimiento
{
    internal const int Filas = 5000;

    /// <summary>Un lote lleno indica que pueden quedar filas pendientes.</summary>
    internal static bool QuedanPendientes(int procesadas) => procesadas == Filas;
}
