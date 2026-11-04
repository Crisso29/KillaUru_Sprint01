namespace KillaUru.Dashboard.Application;

internal sealed class DashboardOptions
{
    internal const string Seccion = "Dashboard";

    public int     DiasConductorActivo { get; init; } = 7;
    public TimeOnly HoraCalculoLocal   { get; init; } = new(0, 30);
    public int     DesfaseHorarioHoras { get; init; } = -5;
    public int     DiasRecuperables    { get; init; } = 2;
    public int     RetencionDias       { get; init; } = 730;
    public int     DiasComparacion     { get; init; } = 7;
    public string  UrlPanelesTecnicos  { get; init; } = "";
}
