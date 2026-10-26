using System.ComponentModel.DataAnnotations;
using KillaUru.Identidad.Application;
namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Consulta paginada; ids usa el formato CSV acordado en OpenAPI.</summary>
internal sealed class ConsultaUsuarios
{
    /// <summary>Fragmento literal del correo.</summary><example>ejemplo.pe</example>
    [MaxLength(100)] public string? Busqueda { get; init; }
    /// <summary>Rol exacto.</summary><example>Administrador</example>
    public string? Rol { get; init; }
    /// <summary>Estado visible de la cuenta.</summary><example>Activa</example>
    public string? Estado { get; init; }
    /// <summary>Hasta 200 UUID separados por comas.</summary><example>019a10e0-0000-7000-8000-000000000001</example>
    [MaxLength(7400)] public string? Ids { get; init; }
    /// <summary>Página desde uno.</summary><example>1</example>
    [Range(1, int.MaxValue)] public int Pagina { get; init; } = 1;
    /// <summary>Tamaño máximo de 200.</summary><example>50</example>
    [Range(1, 200)] public int Tamano { get; init; } = 50;

    internal FiltrosUsuarios? Filtros()
    {
        Guid[]? ids = null;
        if (Ids is not null)
        {
            var partes = Ids.Split(',');
            if (partes.Length > 200 || partes.Any(x => !Guid.TryParse(x, out _))) return null;
            ids = partes.Select(Guid.Parse).ToArray();
        }
        return new(Busqueda, Rol, Estado, ids, Pagina, Tamano);
    }
}
