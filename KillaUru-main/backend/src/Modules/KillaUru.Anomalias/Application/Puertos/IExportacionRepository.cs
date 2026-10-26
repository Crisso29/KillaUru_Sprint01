using KillaUru.Anomalias.Application.Exportacion;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Conteo y lectura comparten instantánea para auditar el mismo conjunto exportado.</summary>
internal interface IExportacionRepository
{
    Task<IAsyncDisposable> AbrirInstantaneaAsync(CancellationToken ct);
    Task<int> ContarAsync(SolicitudExportacion filtros, CancellationToken ct);
    IAsyncEnumerable<FilaExportacion> LeerAsync(SolicitudExportacion filtros, CancellationToken ct);
}
