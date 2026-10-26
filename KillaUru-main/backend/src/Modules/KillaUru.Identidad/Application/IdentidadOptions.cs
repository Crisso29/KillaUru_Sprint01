namespace KillaUru.Identidad.Application;

/// <summary>Parámetros calibrables de identidad; los secretos llegan del entorno.</summary>
internal sealed class IdentidadOptions
{
    public int TokenAccesoMinutos { get; init; } = 5;
    public int SesionAdministradorHoras { get; init; } = 8;
    public int SesionConductorDias { get; init; } = 30;
    public int GraciaRotacionSegundos { get; init; } = 60;
    public int IntentosMaximos { get; init; } = 3;
    public int VentanaIntentosMinutos { get; init; } = 15;
    public int[] DuracionesBloqueoMinutos { get; init; } = [5, 15, 60];
    public int VerificacionCorreoHoras { get; init; } = 24;
    public int RecuperacionMinutos { get; init; } = 30;
    public int InvitacionHoras { get; init; } = 24;
    public int RecuperacionesPorCorreoPorHora { get; init; } = 3;
    public int RecuperacionesPorIpPorHora { get; init; } = 10;
    public int ContrasenaMinima { get; init; } = 10;
    public int ContrasenaMaxima { get; init; } = 128;
    public int RetencionIdentidadDias { get; init; } = 30;
    public int TruncarUsoTrasMinutos { get; init; } = 2;
    public TimeOnly HoraPurgaIdentidad { get; init; } = new(3, 45);
    public string VersionPolitica { get; init; } = "1.2";
    public string VersionTerminos { get; init; } = "1.2";
    public Uri UrlPolitica { get; init; } = new("https://killauru.pe/privacidad");
    public Uri UrlTerminos { get; init; } = new("https://killauru.pe/terminos");
    public string UrlBaseEnlaces { get; init; } = "https://panel.killauru.pe/cuenta";
    public string ClaveRotacion { get; init; } = "";
    public string TurnstileSecreto { get; init; } = "";
    public string CorreoApiKey { get; init; } = "";
    public string CorreoRemitente { get; init; } = "KillaUru <cuentas@killauru.pe>";
    public bool CorreoConsola { get; init; }
    internal bool ContrasenaValida(string? valor) => valor is not null && valor.Length >= ContrasenaMinima && valor.Length <= ContrasenaMaxima;
}
