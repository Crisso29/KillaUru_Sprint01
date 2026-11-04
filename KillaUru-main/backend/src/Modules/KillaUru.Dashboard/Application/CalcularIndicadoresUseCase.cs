using KillaUru.Anomalias.Contracts;
using KillaUru.Dashboard.Application.Puertos;
using KillaUru.Dashboard.Domain;
using KillaUru.Identidad.Contracts;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Dashboard.Application;

internal sealed class CalcularIndicadoresUseCase(
    IIndicadoresRepositorio repositorio,
    IAnomaliaLectura anomalias,
    IIdentidadConsultas identidad,
    IOptions<DashboardOptions> opciones,
    CalculadoUtcHolder utcHolder,
    IMetricas metricas,
    TimeProvider reloj)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        var opts = opciones.Value;
        var hoy  = DiaLocal.Hoy(reloj.GetUtcNow(), opts.DesfaseHorarioHoras);
        var desde = hoy.AddDays(-opts.DiasRecuperables);
        var hasta = hoy.AddDays(-1);
        var guardados = await repositorio.DiasCompletosAsync(desde, hasta, ct);
        var faltantes = DiaLocal.DiasFaltantes(guardados, hoy, opts.DiasRecuperables);
        foreach (var dia in faltantes)
            await CalcularDiaAsync(dia, opts.DiasConductorActivo, ct);
        await repositorio.PurgarAsync(hoy.AddDays(-opts.RetencionDias), ct);
        utcHolder.Actualizar(reloj.GetUtcNow());
        metricas.RegistrarTareaCorrecta("indicadores");
    }

    private async Task CalcularDiaAsync(DateOnly dia, int diasConductor, CancellationToken ct)
    {
        var resumen     = await anomalias.ResumenDelDiaAsync(dia, ct);
        var conductores = await identidad.ContarConductoresActivosAsync(diasConductor, ct);
        await repositorio.GuardarDiaAsync(MapeoResumenDia.Desde(resumen, conductores), ct);
    }
}
