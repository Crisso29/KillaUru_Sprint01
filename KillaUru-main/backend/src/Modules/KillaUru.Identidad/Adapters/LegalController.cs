using KillaUru.Identidad.Application.Legal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KillaUru.Identidad.Adapters;

/// <summary>Publicación anónima de documentos legales vigentes (FR-003).</summary>
[Route("api/v1/legal")]
[AllowAnonymous]
internal sealed class LegalController(ObtenerVersionesLegalesUseCase obtener) : IdentidadControllerBase
{
    /// <summary>Versiones vigentes de la política y los términos.</summary>
    /// <remarks>Rol: público. La aceptación explícita se envía en el registro.</remarks>
    [HttpGet("versiones", Name = "obtenerVersionesLegales")]
    [ProducesResponseType(typeof(VersionesLegales), 200)]
    public async Task<IActionResult> Versiones(CancellationToken ct) => Ok(await obtener.EjecutarAsync(ct));
}
