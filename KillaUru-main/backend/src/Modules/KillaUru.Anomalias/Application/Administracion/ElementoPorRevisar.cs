namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Datos de ElementoPorRevisar para administración; no expone seudónimos.</summary>
internal sealed record ElementoPorRevisar(Guid Id, string Tipo, float KmVia, int SeveridadMostrada, int Vehiculos7d, int Reportes, DateTimeOffset UltimaDeteccionUtc);
