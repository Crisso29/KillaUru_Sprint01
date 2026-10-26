using System.Text;
using KillaUru.Identidad.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
namespace KillaUru.Identidad.Infrastructure;

/// <summary>Impide operar con secretos vacíos o parámetros que debiliten las políticas acordadas.</summary>
internal sealed class ValidadorIdentidadOptions(IHostEnvironment entorno) : IValidateOptions<IdentidadOptions>
{
    public ValidateOptionsResult Validate(string? name, IdentidadOptions opciones)
    {
        if (Encoding.UTF8.GetByteCount(opciones.ClaveRotacion) < 32)
            return ValidateOptionsResult.Fail("Identidad:ClaveRotacion requiere al menos 32 bytes.");
        if (opciones.ContrasenaMinima < 10 || opciones.ContrasenaMaxima > 128 || opciones.ContrasenaMaxima < opciones.ContrasenaMinima
            || opciones.TokenAccesoMinutos is < 1 or > 5 || opciones.IntentosMaximos is < 1 or > short.MaxValue
            || opciones.DuracionesBloqueoMinutos.Length == 0 || opciones.DuracionesBloqueoMinutos.Any(x => x <= 0))
        { return ValidateOptionsResult.Fail("Parámetros de seguridad de Identidad fuera de los límites permitidos."); }
        if (ValoresPositivos(opciones).Any(x => x <= 0)) return ValidateOptionsResult.Fail("Las ventanas y cuotas de Identidad deben ser positivas.");
        if (entorno.IsProduction() && (opciones.CorreoConsola || string.IsNullOrWhiteSpace(opciones.CorreoApiKey)
            || string.IsNullOrWhiteSpace(opciones.TurnstileSecreto)))
        { return ValidateOptionsResult.Fail("Producción requiere correo HTTP y secretos de correo y Turnstile."); }
        return ValidateOptionsResult.Success;
    }

    private static int[] ValoresPositivos(IdentidadOptions o) =>
        [o.SesionAdministradorHoras, o.SesionConductorDias, o.GraciaRotacionSegundos, o.VentanaIntentosMinutos,
        o.VerificacionCorreoHoras, o.RecuperacionMinutos, o.InvitacionHoras, o.RecuperacionesPorCorreoPorHora,
        o.RecuperacionesPorIpPorHora, o.RetencionIdentidadDias, o.TruncarUsoTrasMinutos];
}
