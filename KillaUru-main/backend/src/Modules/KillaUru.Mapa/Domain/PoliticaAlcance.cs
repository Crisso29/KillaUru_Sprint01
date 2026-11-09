using KillaUru.Shared.Domain;
namespace KillaUru.Mapa.Domain;
/// <summary>
/// Aplica RN-08: decide el plan de consulta según rol, filtros y parámetros de la petición.
/// RF-02, RF-11; T-SEG20.
/// </summary>
internal static class PoliticaAlcance
{
    private static readonly EstadoMapa[] TodosLosEstados =
    [
        EstadoMapa.Activa, EstadoMapa.PendienteConfirmacion,
        EstadoMapa.Expirada, EstadoMapa.Reparada, EstadoMapa.Descartada,
    ];
    private static readonly EstadoMapa[] SoloActiva = [EstadoMapa.Activa];
    /// <summary>
    /// Devuelve el <see cref="PlanConsulta"/> que corresponde.
    /// <para><paramref name="estados"/> solo se usa cuando el rol es Administrador y <paramref name="hayEstado"/> es <c>true</c>.</para>
    /// </summary>
    internal static PlanConsulta Decidir(string rol, bool hayFiltros, bool hayEstado,
        bool hayUpdatedSince, EstadoMapa[]? estados = null)
    {
        // estado reservado para el administrador → 403 para los demás roles (T-SEG20)
        if (hayEstado && rol != Roles.Administrador)
            return new PlanConsultaNoPermitido();
        return rol switch
        {
            Roles.Administrador when hayUpdatedSince => new PlanConsultaCambios(RecortarDemo: false),
            Roles.Administrador => new PlanConsultaFiltrada(estados ?? TodosLosEstados, RecortarDemo: false),
            Roles.ConductorRegistrado when hayUpdatedSince => new PlanConsultaCambios(RecortarDemo: false),
            Roles.ConductorRegistrado when hayFiltros => new PlanConsultaFiltrada(SoloActiva, RecortarDemo: false),
            Roles.ConductorRegistrado => new PlanConsultaPorCeldas(Alcance.Registrado),
            _ when hayUpdatedSince => new PlanConsultaCambios(RecortarDemo: true),
            _ when hayFiltros => new PlanConsultaFiltrada(SoloActiva, RecortarDemo: true),
            _ => new PlanConsultaPorCeldas(Alcance.Anonimo),
        };
    }
}
