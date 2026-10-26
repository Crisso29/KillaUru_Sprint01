namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Sesión emitida o rechazo; el bloqueo solo se expone al panel.</summary>
internal sealed record ResultadoLogin(SesionEmitida? Sesion = null, string? Error = null,
    int Estado = 200, DateTimeOffset? BloqueadoHastaUtc = null);
