namespace KillaUru.Identidad.Application.Cuenta;

/// <summary>Aceptaciones explícitas; una versión ausente nunca equivale a consentimiento.</summary>
internal sealed record RegistroEntrada(string Correo, string Contrasena, string? VersionPoliticaAceptada, string? VersionTerminosAceptada);
