using System.ComponentModel.DataAnnotations;
using KillaUru.Auditoria.Application;
namespace KillaUru.Auditoria.Adapters.Dtos;

/// <summary>Filtros HTTP opcionales; la aplicación vuelve a validar al recibirlos.</summary>
internal sealed class ConsultaAuditoriaDto
{
    /// <summary>Inicio inclusivo del rango.</summary><example>2026-10-01T00:00:00Z</example>
    public DateTimeOffset? Desde { get; init; }
    /// <summary>Fin inclusivo del rango.</summary><example>2026-10-31T23:59:59Z</example>
    public DateTimeOffset? Hasta { get; init; }
    /// <summary>Categoría del catálogo.</summary><example>Sesion</example>
    public string? Categoria { get; init; }
    /// <summary>Nombre exacto de la acción.</summary><example>CuentaBloqueada</example>
    public string? Accion { get; init; }
    /// <summary>Identificador interno del actor.</summary><example>7f3a2c1e-0000-4000-8000-000000000001</example>
    public Guid? ActorId { get; init; }
    /// <summary>Página desde uno.</summary><example>1</example>
    [Range(1, int.MaxValue)] public int Pagina { get; init; } = 1;
    /// <summary>Registros por página, de uno a doscientos.</summary><example>50</example>
    [Range(1, 200)] public int Tamano { get; init; } = 50;

    internal FiltrosAuditoria AEntrada() => new(Desde, Hasta, Categoria, Accion, ActorId, Pagina, Tamano);
}
