using System.Text;
using Microsoft.Extensions.Options;

namespace KillaUru.Api.Configuracion;

/// <summary>
/// Valida <see cref="ApiOptions"/> al arrancar: una configuración inválida debe impedir que la API
/// arranque, no aparecer como un error raro en producción.
/// </summary>
internal sealed class ValidadorApiOptions : IValidateOptions<ApiOptions>
{
    private const int BytesMinimosClave = 32;

    public ValidateOptionsResult Validate(string? name, ApiOptions options)
    {
        var errores = new List<string>();
        ValidarCuotas(options.LimiteTasa, errores);
        ValidarSeguridad(options, errores);
        if (options.Telemetria.MuestreoTrazas is < 0 or > 0.10)
        {
            errores.Add("Api:Telemetria:MuestreoTrazas debe estar entre 0 y 0,10 (RNF-32).");
        }
        return errores.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errores);
    }

    /// <summary>
    /// Verifica que todas las cuotas de límite de tasa sean positivas (RNF-11).
    /// Una cuota de 0 bloquearía a todos los usuarios del rol correspondiente.
    /// </summary>
    private static void ValidarCuotas(OpcionesLimiteTasa limites, List<string> errores)
    {
        int[] cuotas =
        [
            limites.AnonimoConsultasPorHora,
            limites.AnonimoLotesPorHora,
            limites.RegistradoPorHora,
            limites.AdministradorPorHora,
            limites.TechoIpLotesPorHora,
            limites.TechoIpConsultasPorHora,
            limites.RegistrosPorHoraPorIp,
        ];

        if (cuotas.Any(c => c <= 0))
        {
            errores.Add("Api:LimiteTasa: todas las cuotas deben ser mayores que 0.");
        }
    }

    /// <summary>
    /// Verifica la clave de firma JWT y la URL del panel (ADR-009, RNF-09).
    /// Una clave corta es vulnerable a fuerza bruta; una URL de panel inválida bloquea CORS.
    /// </summary>
    private static void ValidarSeguridad(ApiOptions options, List<string> errores)
    {
        var bytesClave = Encoding.UTF8.GetByteCount(options.Jwt.ClaveFirma);
        if (bytesClave < BytesMinimosClave)
        {
            errores.Add($"Api:Jwt:ClaveFirma debe tener al menos {BytesMinimosClave} bytes (KU_JWT_CLAVE_FIRMA).");
        }

        if (!Uri.TryCreate(options.OrigenPanel, UriKind.Absolute, out _))
        {
            errores.Add("Api:OrigenPanel debe ser una URL absoluta.");
        }
    }
}
