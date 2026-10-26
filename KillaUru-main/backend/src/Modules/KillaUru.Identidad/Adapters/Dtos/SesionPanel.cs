namespace KillaUru.Identidad.Adapters.Dtos;

/// <summary>Respuesta del panel sin tokens en el cuerpo: ambos se entregan únicamente en cookies.</summary>
/// <param name="ExpiraAccesoUtc">Vigencia del acceso. Ejemplo: 2026-10-23T10:05:00Z.</param>
/// <param name="SesionVenceUtc">Fin máximo de la familia de sesión.</param>
/// <param name="Rol">Rol autenticado. Ejemplo: Administrador.</param>
internal sealed record SesionPanel(DateTimeOffset ExpiraAccesoUtc, DateTimeOffset SesionVenceUtc, string Rol);
