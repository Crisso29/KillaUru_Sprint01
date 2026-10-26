namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Par de credenciales que el adaptador entrega por cuerpo o cookies, nunca por logs.</summary>
internal sealed record SesionEmitida(string TokenAcceso, DateTimeOffset ExpiraAccesoUtc,
    string TokenRenovacion, DateTimeOffset SesionVenceUtc, string Rol);
