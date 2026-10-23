namespace KillaUru.Auditoria.Domain;

/// <summary>Resultado de validación: nunca transporta los valores personales rechazados.</summary>
internal sealed record ResultadoRegistro(RegistroAuditoria? Registro, string? Error);
