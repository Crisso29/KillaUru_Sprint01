namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Proyección para administradores; no expone hashes ni estado de bloqueo.</summary>
internal sealed record UsuarioVista(Guid Id, string Correo, string Rol, string Estado,
    bool CorreoVerificado, DateTimeOffset FechaAltaUtc, DateTimeOffset? UltimaSesionUtc);
