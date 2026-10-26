namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Datos de DetalleAnomalia para administración; no expone seudónimos.</summary>
internal sealed record DetalleAnomalia(Guid Id, string Tipo, int SeveridadMostrada, string Estado, string Persistencia, double Latitud, double Longitud, float KmVia, int DiasDistintos15d, int Reportes, DateTimeOffset UltimaDeteccionUtc, DateTimeOffset? VenceUtc, bool VisibleConductores, bool PorRevisar, int VehiculosRevision, IReadOnlyList<ConteoDia> ReportesPorDia, IReadOnlyList<EstadoHistorico> Historial);
