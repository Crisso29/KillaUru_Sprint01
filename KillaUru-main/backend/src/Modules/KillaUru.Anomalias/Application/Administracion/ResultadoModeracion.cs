namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Datos de ResultadoModeracion para administración; no expone seudónimos.</summary>
internal sealed record ResultadoModeracion(DetalleAnomalia? Detalle, string? Error = null);
