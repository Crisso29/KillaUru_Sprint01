namespace KillaUru.Anomalias.Domain;
/// <summary>Parámetros calibrables de RN-05 a RN-15; valores iniciales del catálogo de negocio.</summary>
internal sealed class AnomaliasOptions
{
    public double ConfidenceMinimo { get; set; } = 0.75;
    public int PrecisionGpsMaximaMetros { get; set; } = 20;
    public int AntiguedadMaximaDias { get; set; } = 7;
    public int ToleranciaFuturoMinutos { get; set; } = 5;
    public int RadioAgrupacionMetros { get; set; } = 100;
    public int VentanaSeveridadDias { get; set; } = 15;
    public int SeveridadAltaDesde { get; set; } = 4;
    public int DispositivosRequeridos { get; set; } = 2;
    public int VentanaConfirmacionHoras { get; set; } = 72;
    public int SeparacionMinimaMinutos { get; set; } = 3;
    public int ReportesConSesionRequeridos { get; set; } = 1;
    public int MaxEventosPorHoraDeteccion { get; set; } = 120;
    public int VelocidadMaximaPlausibleKmh { get; set; } = 150;
    public int SeparacionMinimaPlausibilidadSegundos { get; set; } = 10;
    public int DiasDistintosMinimos { get; set; } = 3;
    public int VentanaDias { get; set; } = 15;
    public int DesfaseHorarioHoras { get; set; } = -5;
    public int CaducidadEpisodicaHoras { get; set; } = 72;
    public int CaducidadPersistenteDias { get; set; } = 7;
    public int CaducidadPendienteHoras { get; set; } = 72;
    public int VehiculosRevisionDescartada { get; set; } = 3;
    public int VentanaRevisionDescartadaDias { get; set; } = 7;
    public int PurgaSeudonimoHoras { get; set; } = 72;
    public int RetencionHistoricaDias { get; set; } = 730;
    public int DistanciaMaximaViaMetros { get; set; } = 50;
}
