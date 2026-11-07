using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using Microsoft.Extensions.Options;

namespace KillaUru.Dashboard.Application;

internal sealed class ConsultarResumenUseCase(
    IIndicadoresRepositorio repositorio,
    CalculadoUtcHolder utcHolder,
    IOptions<DashboardOptions> opciones,
    TimeProvider reloj)
{
    internal async Task<ResumenDashboard> EjecutarAsync(int dias, CancellationToken ct)
    {
        var opts     = opciones.Value;
        var hoy      = DiaLocal.Hoy(reloj.GetUtcNow(), opts.DesfaseHorarioHoras);
        var d        = hoy.AddDays(-1);
        var diasLeer = dias * 2 + 7;
        var desde    = d.AddDays(-(diasLeer - 1));
        var datos    = await repositorio.LeerAsync(desde, d, ct);

        return MapeadorResumen.Crear(datos, desde, dias, utcHolder.UltimaCalculacion, opts.UrlPanelesTecnicos);
    }
}

