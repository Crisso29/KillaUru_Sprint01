using KillaUru.Anomalias.Contracts;
using KillaUru.Shared.Domain;

namespace KillaUru.Mapa.Domain;

/// <summary>
/// Aplica RN-08: decide el plan de consulta según rol, filtros y parámetros de la petición.
/// RF-02, RF-11; T-SEG20.
/// </summary>
internal static class PoliticaAlcance
{
    private static readonly EstadoAnomalia[] TodosLosEstados =
    [
        EstadoAnomalia.Activa, EstadoAnomalia.PendienteConfirmacion,
        EstadoAnomalia.Expirada, EstadoAnomalia.Reparada, EstadoAnomalia.Descartada,
    ];

    private static readonly EstadoAnomalia[] SoloActiva = [EstadoAnomalia.Activa];

    /// <summary>
    /// Devuelve el <see cref="PlanConsulta"/> que corresponde.
    /// <para><paramref name="estados"/> solo se usa cuando el rol es Administrador y <paramref name="hayEstado"/> es <c>true</c>.</para>
    /// </summary>
    internal static PlanConsulta Decidir(string rol, bool hayFiltros, bool hayEstado,
        bool hayUpdatedSince, EstadoAnomalia[]? estados = null)
    {
        // estado reservado para el administrador → 403 para los demás roles (T-SEG20)
        if (hayEstado && rol != Roles.Administrador)
            return new PlanConsulta.NoPermitido();

        return rol switch
        {
            Roles.Administrador when hayUpdatedSince =>
                new PlanConsulta.Cambios(RecortarDemo: false),

            Roles.Administrador =>
                new PlanConsulta.Filtrada(estados ?? TodosLosEstados, RecortarDemo: false),

            Roles.ConductorRegistrado when hayUpdatedSince =>
                new PlanConsulta.Cambios(RecortarDemo: false),

            Roles.ConductorRegistrado when hayFiltros =>
                new PlanConsulta.Filtrada(SoloActiva, RecortarDemo: false),

            Roles.ConductorRegistrado =>
                new PlanConsulta.PorCeldas(Alcance.Registrado),

            _ when hayUpdatedSince =>
                new PlanConsulta.Cambios(RecortarDemo: true),

            _ when hayFiltros =>
                new PlanConsulta.Filtrada(SoloActiva, RecortarDemo: true),

            _ =>
                new PlanConsulta.PorCeldas(Alcance.Anonimo),
        };
    }
}
