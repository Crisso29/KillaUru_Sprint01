namespace KillaUru.Identidad.Application;

/// <summary>Filtros validados antes de ejecutar consultas de datos personales.</summary>
internal sealed record FiltrosUsuarios(string? Busqueda = null, string? Rol = null, string? Estado = null,
    Guid[]? Ids = null, int Pagina = 1, int Tamano = 50)
{
    internal bool Validos => Pagina >= 1 && Tamano is >= 1 and <= 200 && (long)(Pagina - 1) * Tamano <= int.MaxValue
        && (Busqueda?.Length ?? 0) <= 100 && (Ids?.Length ?? 0) <= 200
        && Rol is null or "Administrador" or "ConductorRegistrado"
        && Estado is null or "Activa" or "Desactivada" or "InvitacionPendiente";
}
