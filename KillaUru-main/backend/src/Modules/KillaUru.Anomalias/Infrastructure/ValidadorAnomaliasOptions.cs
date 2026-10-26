using KillaUru.Anomalias.Domain;
using Microsoft.Extensions.Options;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Rechaza parámetros imposibles antes de recibir eventos; evita ventanas negativas y conteos inválidos.</summary>
internal sealed class ValidadorAnomaliasOptions : IValidateOptions<AnomaliasOptions>
{
    public ValidateOptionsResult Validate(string? name, AnomaliasOptions o)
    {
        var positivos = typeof(AnomaliasOptions).GetProperties().Where(p => p.PropertyType == typeof(int)
            && p.Name != nameof(o.DesfaseHorarioHoras)).All(p => (int)p.GetValue(o)! > 0);
        if (!positivos || o.ConfidenceMinimo is < 0 or > 1 || !double.IsFinite(o.ConfidenceMinimo)
            || o.SeveridadAltaDesde is < 1 or > 5 || o.DispositivosRequeridos < 2
            || o.ReportesConSesionRequeridos > o.DispositivosRequeridos || o.DesfaseHorarioHoras != -5
            || o.PurgaSeudonimoHoras < 2 || o.RadioAgrupacionMetros > 200)
        { return ValidateOptionsResult.Fail("Parámetros Anomalias inválidos; revisar ventanas, umbrales y radio de celdas."); }
        return ValidateOptionsResult.Success;
    }
}
