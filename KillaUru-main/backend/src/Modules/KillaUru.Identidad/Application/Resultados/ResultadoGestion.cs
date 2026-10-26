namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Resultado de negocio explícito; los fallos esperados no usan excepciones.</summary>
internal sealed record ResultadoGestion(string? Error = null, int Estado = 204);
