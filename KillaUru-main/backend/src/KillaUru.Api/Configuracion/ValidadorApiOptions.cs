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
        var limites = options.LimiteTasa;
        int[] cuotas =
        [
            limites.AnonimoConsultasPorHora, limites.AnonimoLotesPorHora, limites.RegistradoPorHora,
            limites.AdministradorPorHora, limites.TechoIpLotesPorHora, limites.TechoIpConsultasPorHora,
            limites.RegistrosPorHoraPorIp,
        ];
        if (cuotas.Any(c => c <= 0))
        {
            errores.Add("Api:LimiteTasa: todas las cuotas deben ser mayores que 0.");
        }

        if (options.Telemetria.MuestreoTrazas is < 0 or > 0.10)
        {
            errores.Add("Api:Telemetria:MuestreoTrazas debe estar entre 0 y 0,10 (RNF-32).");
        }

        if (Encoding.UTF8.GetByteCount(options.Jwt.ClaveFirma) < BytesMinimosClave)
        {
            errores.Add($"Api:Jwt:ClaveFirma debe tener al menos {BytesMinimosClave} bytes (KU_JWT_CLAVE_FIRMA).");
        }

        if (!Uri.TryCreate(options.OrigenPanel, UriKind.Absolute, out _))
        {
            errores.Add("Api:OrigenPanel debe ser una URL absoluta.");
        }

        return errores.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errores);
    }
}
