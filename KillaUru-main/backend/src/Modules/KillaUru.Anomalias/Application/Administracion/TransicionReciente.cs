namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Datos de TransicionReciente para administración; no expone seudónimos.</summary>
internal sealed record TransicionReciente(Guid AnomaliaId, string Tipo, float KmVia, string? Desde, string Hacia, DateTimeOffset OcurridaUtc, Guid? ActorId);
