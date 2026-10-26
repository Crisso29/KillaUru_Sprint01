namespace KillaUru.Identidad.Application.Sesion;

/// <summary>Canal explícito para aplicar anti-bot y no revelar bloqueos a conductores.</summary>
internal sealed record InicioSesionEntrada(string Correo, string Contrasena, string Canal, string? TurnstileToken);
