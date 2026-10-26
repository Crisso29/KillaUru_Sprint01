namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Datos de EstadoHistorico para administración; no expone seudónimos.</summary>
internal sealed record EstadoHistorico(string? Desde, string Hacia, DateTimeOffset OcurridaUtc, Guid? ActorId);
