namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Estado mensual y conjunto de administradores sujetos a revisión.</summary>
internal sealed record EstadoRevision(DateTimeOffset? UltimaRevisionUtc, DateTimeOffset VenceUtc,
    IReadOnlyList<UsuarioVista> Administradores);
