using KillaUru.Mapa.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Mapa.Tests.Domain;

/// <summary>
/// PoliticaAlcance decide el plan de consulta según el rol y los parámetros presentes.
/// RN-08, RF-14, RF-17; T-SEG20. Spec 006 · FR-002, FR-011.
/// </summary>
public sealed class PoliticaAlcanceTests
{
    // ── Anónimo ─────────────────────────────────────────────────────────────────

    /// <summary>Anónimo sin filtros → PorCeldas(Anonimo).</summary>
    [Fact]
    public void Anonimo_sin_filtros_usa_PorCeldas_Anonimo()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.Anonimo, hayFiltros: false, hayEstado: false,
            hayUpdatedSince: false);

        // Assert
        var porCeldas = Assert.IsType<PlanConsultaPorCeldas>(plan);
        Assert.Equal(Alcance.Anonimo, porCeldas.Alcance);
    }

    /// <summary>Anónimo con filtros de tipo/severidad → Filtrada([Activa], recortarDemo=true).</summary>
    [Fact]
    public void Anonimo_con_filtros_usa_Filtrada_recortarDemo()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.Anonimo, hayFiltros: true, hayEstado: false,
            hayUpdatedSince: false);

        // Assert
        var filtrada = Assert.IsType<PlanConsultaFiltrada>(plan);
        Assert.True(filtrada.RecortarDemo);
        Assert.Contains(EstadoMapa.Activa, filtrada.Estados);
        Assert.Single(filtrada.Estados);
    }

    /// <summary>Anónimo con updatedSince → Cambios(recortarDemo=true).</summary>
    [Fact]
    public void Anonimo_con_updatedSince_usa_Cambios_recortarDemo()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.Anonimo, hayFiltros: false, hayEstado: false,
            hayUpdatedSince: true);

        // Assert
        var cambios = Assert.IsType<PlanConsultaCambios>(plan);
        Assert.True(cambios.RecortarDemo);
    }

    /// <summary>Anónimo con estado → NoPermitido (T-SEG20).</summary>
    [Fact]
    public void Anonimo_con_estado_devuelve_NoPermitido()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.Anonimo, hayFiltros: false, hayEstado: true,
            hayUpdatedSince: false);

        // Assert
        Assert.IsType<PlanConsultaNoPermitido>(plan);
    }

    // ── Conductor registrado ────────────────────────────────────────────────────

    /// <summary>Conductor sin filtros → PorCeldas(Registrado).</summary>
    [Fact]
    public void Conductor_sin_filtros_usa_PorCeldas_Registrado()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.ConductorRegistrado, hayFiltros: false,
            hayEstado: false, hayUpdatedSince: false);

        // Assert
        var porCeldas = Assert.IsType<PlanConsultaPorCeldas>(plan);
        Assert.Equal(Alcance.Registrado, porCeldas.Alcance);
    }

    /// <summary>Conductor con filtros → Filtrada([Activa], recortarDemo=false).</summary>
    [Fact]
    public void Conductor_con_filtros_usa_Filtrada_sin_recortarDemo()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.ConductorRegistrado, hayFiltros: true,
            hayEstado: false, hayUpdatedSince: false);

        // Assert
        var filtrada = Assert.IsType<PlanConsultaFiltrada>(plan);
        Assert.False(filtrada.RecortarDemo);
        Assert.Single(filtrada.Estados);
        Assert.Contains(EstadoMapa.Activa, filtrada.Estados);
    }

    /// <summary>Conductor con updatedSince → Cambios(recortarDemo=false).</summary>
    [Fact]
    public void Conductor_con_updatedSince_usa_Cambios_sin_recortarDemo()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.ConductorRegistrado, hayFiltros: false,
            hayEstado: false, hayUpdatedSince: true);

        // Assert
        var cambios = Assert.IsType<PlanConsultaCambios>(plan);
        Assert.False(cambios.RecortarDemo);
    }

    /// <summary>Conductor con estado → NoPermitido (T-SEG20).</summary>
    [Fact]
    public void Conductor_con_estado_devuelve_NoPermitido()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.ConductorRegistrado, hayFiltros: false,
            hayEstado: true, hayUpdatedSince: false);

        // Assert
        Assert.IsType<PlanConsultaNoPermitido>(plan);
    }

    // ── Administrador ───────────────────────────────────────────────────────────

    /// <summary>Administrador sin filtros → Filtrada([5 estados], recortarDemo=false).</summary>
    [Fact]
    public void Administrador_sin_filtros_usa_Filtrada_todos_estados()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.Administrador, hayFiltros: false, hayEstado: false,
            hayUpdatedSince: false);

        // Assert
        var filtrada = Assert.IsType<PlanConsultaFiltrada>(plan);
        Assert.False(filtrada.RecortarDemo);
        Assert.Equal(5, filtrada.Estados.Length);
    }

    /// <summary>Administrador con estados → Filtrada con los estados pedidos.</summary>
    [Fact]
    public void Administrador_con_estados_usa_los_estados_pedidos()
    {
        // Arrange
        var estados = new[] { EstadoMapa.Activa, EstadoMapa.PendienteConfirmacion };

        // Act
        var plan = PoliticaAlcance.Decidir(Roles.Administrador, hayFiltros: false, hayEstado: true,
            hayUpdatedSince: false, estados: estados);

        // Assert
        var filtrada = Assert.IsType<PlanConsultaFiltrada>(plan);
        Assert.Equal(estados, filtrada.Estados);
    }

    /// <summary>Administrador con updatedSince → Cambios(recortarDemo=false).</summary>
    [Fact]
    public void Administrador_con_updatedSince_usa_Cambios()
    {
        // Arrange / Act
        var plan = PoliticaAlcance.Decidir(Roles.Administrador, hayFiltros: false, hayEstado: false,
            hayUpdatedSince: true);

        // Assert
        var cambios = Assert.IsType<PlanConsultaCambios>(plan);
        Assert.False(cambios.RecortarDemo);
    }
}
