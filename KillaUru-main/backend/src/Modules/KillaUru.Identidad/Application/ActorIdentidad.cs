namespace KillaUru.Identidad.Application;

/// <summary>Identidad autenticada recibida del adaptador, sin dependencia de HttpContext.</summary>
internal sealed record ActorIdentidad(Guid Id, string Rol);
