using System.Text;
using KillaUru.Identidad.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Infrastructure;

/// <summary>
/// Falla al arrancar si un secreto falta o un parámetro debilita las políticas acordadas: es mejor
/// no levantar la API que levantarla insegura. Cada regla devuelve su propio mensaje.
/// </summary>
internal sealed class ValidadorIdentidadOptions(IHostEnvironment entorno) : IValidateOptions<IdentidadOptions>
{
    /// <summary>HMAC-SHA256 necesita una clave de al menos 32 bytes para no perder fuerza.</summary>
    private const int BytesMinimosClaveRotacion = 32;

    public ValidateOptionsResult Validate(string? name, IdentidadOptions opciones)
    {
        var error = ErrorDeClave(opciones) ?? ErrorDeContrasena(opciones) ?? ErrorDeSesionYBloqueo(opciones)
            ?? ErrorDeVentanas(opciones) ?? ErrorDeProduccion(opciones);
        return error is null ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(error);
    }

    private static string? ErrorDeClave(IdentidadOptions opciones) =>
        Encoding.UTF8.GetByteCount(opciones.ClaveRotacion) < BytesMinimosClaveRotacion
            ? "Identidad:ClaveRotacion requiere al menos 32 bytes." : null;

    /// <summary>D13: mínimo 10; máximo 128 para que PBKDF2 no sirva de amplificador de CPU.</summary>
    private static string? ErrorDeContrasena(IdentidadOptions opciones)
    {
        var minimoValido = opciones.ContrasenaMinima >= 10;
        var maximoValido = opciones.ContrasenaMaxima <= 128 && opciones.ContrasenaMaxima >= opciones.ContrasenaMinima;
        return minimoValido && maximoValido ? null : "Identidad: longitud de contraseña fuera de 10–128.";
    }

    /// <summary>El JWT de acceso no puede durar más de 5 min (FR-014); el bloqueo necesita al menos una duración.</summary>
    private static string? ErrorDeSesionYBloqueo(IdentidadOptions opciones)
    {
        var accesoValido = opciones.TokenAccesoMinutos is >= 1 and <= 5;
        var intentosValidos = opciones.IntentosMaximos is >= 1 and <= short.MaxValue;
        var duraciones = opciones.DuracionesBloqueoMinutos;
        var duracionesValidas = duraciones.Length > 0 && duraciones.All(minutos => minutos > 0);
        return accesoValido && intentosValidos && duracionesValidas ? null : "Identidad: sesión o bloqueo fuera de los límites permitidos.";
    }

    private static string? ErrorDeVentanas(IdentidadOptions opciones) =>
        ValoresQueDebenSerPositivos(opciones).Any(valor => valor <= 0) ? "Las ventanas y cuotas de Identidad deben ser positivas." : null;

    /// <summary>En producción no se permite el correo por consola ni secretos vacíos.</summary>
    private string? ErrorDeProduccion(IdentidadOptions opciones)
    {
        if (!entorno.IsProduction()) return null;
        var faltaSecreto = string.IsNullOrWhiteSpace(opciones.CorreoApiKey) || string.IsNullOrWhiteSpace(opciones.TurnstileSecreto);
        return opciones.CorreoConsola || faltaSecreto ? "Producción requiere correo HTTP y secretos de correo y Turnstile." : null;
    }

    private static int[] ValoresQueDebenSerPositivos(IdentidadOptions o) =>
        [o.SesionAdministradorHoras, o.SesionConductorDias, o.GraciaRotacionSegundos, o.VentanaIntentosMinutos,
        o.VerificacionCorreoHoras, o.RecuperacionMinutos, o.InvitacionHoras, o.RecuperacionesPorCorreoPorHora,
        o.RecuperacionesPorIpPorHora, o.RetencionIdentidadDias, o.TruncarUsoTrasMinutos];
}
