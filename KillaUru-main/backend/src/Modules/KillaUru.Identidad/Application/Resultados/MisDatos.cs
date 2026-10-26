namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Datos personales del titular, sin secretos de autenticación.</summary>
internal sealed record MisDatos(string Correo, string Rol, DateTimeOffset FechaAltaUtc,
    bool CorreoVerificado, IReadOnlyList<AceptacionVista> Aceptaciones);
