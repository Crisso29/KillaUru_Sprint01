using System.Text.Json.Serialization;
using KillaUru.Auditoria.Application;
namespace KillaUru.Auditoria.Adapters.Dtos;

/// <summary>Página ordenada por hora e identificador descendentes; el total respeta todos los filtros.</summary>
[JsonNumberHandling(JsonNumberHandling.Strict)]
internal sealed class PaginaAuditoriaDto
{
    /// <summary>Registros de la página solicitada.</summary><example>[]</example>
    public required IReadOnlyList<RegistroAuditoriaDto> Elementos { get; init; }
    /// <summary>Página consultada.</summary><example>1</example>
    public required int Pagina { get; init; }
    /// <summary>Máximo de registros por página.</summary><example>50</example>
    public required int Tamano { get; init; }
    /// <summary>Registros que cumplen los filtros, antes de paginar.</summary><example>1042</example>
    public required long Total { get; init; }

    internal static PaginaAuditoriaDto De(PaginaAuditoria pagina) => new()
    {
        Elementos = pagina.Elementos.Select(RegistroAuditoriaDto.De).ToArray(),
        Pagina = pagina.Pagina, Tamano = pagina.Tamano, Total = pagina.Total,
    };
}
