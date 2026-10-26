namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Renovación o razón estable para descartar el permiso local.</summary>
internal sealed record ResultadoRenovacion(SesionEmitida? Sesion = null, string? Error = null);
