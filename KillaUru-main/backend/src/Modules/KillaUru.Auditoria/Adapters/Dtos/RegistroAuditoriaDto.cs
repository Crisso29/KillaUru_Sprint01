using System.Text.Json;
using System.Text.Json.Serialization;
using KillaUru.Auditoria.Application;
namespace KillaUru.Auditoria.Adapters.Dtos;

/// <summary>Representación pública del hecho: no contiene el correo del actor.</summary>
[JsonNumberHandling(JsonNumberHandling.Strict)]
internal sealed class RegistroAuditoriaDto
{
    /// <summary>Identificador del registro.</summary><example>1042</example>
    public required long Id { get; init; }
    /// <summary>Instante UTC del hecho.</summary><example>2026-10-20T14:32:05Z</example>
    public required DateTimeOffset OcurridoUtc { get; init; }
    /// <summary>Cuenta interna; null para el sistema.</summary><example>7f3a2c1e-0000-4000-8000-000000000001</example>
    public Guid? ActorId { get; init; }
    /// <summary>Rol al actuar; Sistema para tareas automáticas.</summary><example>Administrador</example>
    public required string ActorRol { get; init; }
    /// <summary>Agrupación de la acción.</summary><example>Moderacion</example>
    public required string Categoria { get; init; }
    /// <summary>Nombre estable del catálogo.</summary><example>AnomaliaModerada</example>
    public required string Accion { get; init; }
    /// <summary>Recurso afectado; máximo cien caracteres.</summary><example>auditoria</example>
    public string? Objetivo { get; init; }
    /// <summary>Datos no personales del hecho.</summary><example>{"filas":312}</example>
    public required JsonElement Contexto { get; init; }

    internal static RegistroAuditoriaDto De(RegistroConsultado registro) => new()
    {
        Id = registro.Id, OcurridoUtc = registro.OcurridoUtc, ActorId = registro.ActorId,
        ActorRol = registro.ActorRol ?? "Sistema", Categoria = registro.Categoria, Accion = registro.NombreAccion,
        Objetivo = registro.Objetivo, Contexto = JsonSerializer.Deserialize<JsonElement>(registro.Contexto),
    };
}
