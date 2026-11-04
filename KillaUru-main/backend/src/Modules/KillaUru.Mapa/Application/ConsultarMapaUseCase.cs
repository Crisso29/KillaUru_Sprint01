using KillaUru.Mapa.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Mapa.Application;

/// <summary>
/// Despachador principal: aplica PoliticaAlcance y delega en uno de los tres caminos.
/// FR-002; diseño-interno §4.
/// </summary>
internal sealed class ConsultarMapaUseCase(
    ConsultarPorCeldasUseCase porCeldas,
    ConsultarFiltradoUseCase filtrado,
    ConsultarCambiosUseCase cambios)
{
    internal async Task<ResultadoMapa> EjecutarAsync(ConsultaMapa consulta, CancellationToken ct)
    {
        var plan = PoliticaAlcance.Decidir(consulta.Rol, consulta.HayFiltros,
            consulta.HayEstado, consulta.HayUpdatedSince, consulta.Estados);

        return plan switch
        {
            PlanConsulta.NoPermitido =>
                new ResultadoMapa.Prohibido(),

            PlanConsulta.PorCeldas pc =>
                await porCeldas.EjecutarAsync(pc.Alcance, consulta.Bbox,
                    consulta.IfNoneMatch, ct),

            PlanConsulta.Filtrada f =>
                await filtrado.EjecutarAsync(consulta.Bbox, consulta.Tipo, consulta.Severidades,
                    consulta.DesdeUtc, consulta.HastaUtc, f.Estados, f.RecortarDemo, ct),

            PlanConsulta.Cambios c =>
                await cambios.EjecutarAsync(consulta.UpdatedSince!.Value, c.RecortarDemo,
                    consulta.Rol == Roles.Administrador, ct),

            _ => throw new InvalidOperationException("Plan de consulta desconocido."),
        };
    }
}
