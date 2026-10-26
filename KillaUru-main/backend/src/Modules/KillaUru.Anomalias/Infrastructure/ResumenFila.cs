namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Una sola fila agregada para que todos los contadores compartan la misma instantánea SQL.</summary>
internal sealed record ResumenFila(long Eventos, long Dispositivos, int Tramos, long Confirmaciones, long Pendientes, long Expiraciones, long Renacimientos);
