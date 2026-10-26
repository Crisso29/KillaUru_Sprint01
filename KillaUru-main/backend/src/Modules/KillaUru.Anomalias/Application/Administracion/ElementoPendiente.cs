namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Datos de ElementoPendiente para administración; no expone seudónimos.</summary>
internal sealed record ElementoPendiente(Guid Id, string Tipo, float KmVia, int SeveridadMostrada, int Reportes, DateTimeOffset PrimerReporteUtc, DateTimeOffset VenceUtc);
