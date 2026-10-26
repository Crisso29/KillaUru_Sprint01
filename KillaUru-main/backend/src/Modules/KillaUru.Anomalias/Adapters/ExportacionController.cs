using KillaUru.Anomalias.Application.Exportacion;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Anomalias.Adapters;
/// <summary>Entrega un archivo incremental después de confirmar su auditoría.</summary>
internal sealed class ExportacionController(ExportarDatasetUseCase exportar) : AnomaliasControllerBase
{
    /// <summary>Exportar anomalías en CSV o GeoJSON sin seudónimos.</summary>
    /// <remarks>Rol: administrador. Auditoría con formato, filtros y cantidad antes de escribir.</remarks>
    [HttpPost("api/v1/exportaciones", Name = "exportarDataset")]
    [Produces("text/csv", "application/geo+json"), ProducesResponseType(200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Exportar(SolicitudExportacion solicitud, CancellationToken ct)
    {
        Response.ContentType = solicitud.Formato == "CSV" ? "text/csv; charset=utf-8" : "application/geo+json";
        Response.Headers.ContentDisposition = "attachment; filename=anomalias." + (solicitud.Formato == "CSV" ? "csv" : "geojson");
        await exportar.EjecutarAsync(solicitud, Actor, Response.Body, ct);
        return new EmptyResult();
    }
}
