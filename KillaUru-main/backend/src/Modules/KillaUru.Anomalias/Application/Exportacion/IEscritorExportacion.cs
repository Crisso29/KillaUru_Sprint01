namespace KillaUru.Anomalias.Application.Exportacion;
/// <summary>Estrategia de serialización incremental; el destino pertenece al llamador.</summary>
internal interface IEscritorExportacion
{
    string Formato { get; }
    Task EscribirAsync(Stream destino, IAsyncEnumerable<FilaExportacion> filas, CancellationToken ct);
}
