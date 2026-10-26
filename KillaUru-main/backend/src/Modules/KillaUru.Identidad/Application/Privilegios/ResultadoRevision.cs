using KillaUru.Identidad.Application.Resultados;
namespace KillaUru.Identidad.Application.Privilegios;

/// <summary>Resultado de una revisión completa, sin aplicar parcialmente los retiros inválidos.</summary>
internal sealed record ResultadoRevision(EstadoRevision? Revision = null, string? Error = null, int Estado = 201);
