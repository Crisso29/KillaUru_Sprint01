using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Spec 001 — FR-029 (ADR-001, ADR-002, RNF-28, D4, CHK035):
/// monolito modular: un módulo solo accede a otro por su Contracts/, el grafo de proyectos
/// es exactamente el declarado, y Shared no conoce a ningún módulo.
/// Esto garantiza que los módulos puedan evolucionar de forma independiente y eventualmente
/// extraerse como microservicios sin romper contratos.
/// </summary>
public sealed class ReglasDeModulosTests
{
    // Capas cuyas dependencias están prohibidas desde otros módulos (solo Contracts/ es pública)
    private static readonly string[] CapasInternas = ["Domain", "Application", "Adapters", "Infrastructure"];

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void El_csproj_referencia_exactamente_los_modulos_del_grafo(string modulo)
    {
        // Arrange — leer las <ProjectReference> del .csproj del módulo
        var declarados = Repositorio.ModulosReferenciados(modulo)
            .Where(Modulos.Nombres.Contains)
            .Order();

        // Assert — las referencias deben coincidir exactamente con el grafo permitido (ADR-001)
        Assert.Equal(Modulos.GrafoPermitido[modulo].Order(), declarados);
    }

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void El_ensamblado_compilado_no_usa_modulos_fuera_del_grafo(string modulo)
    {
        // Arrange — obtener los ensamblados de módulos referenciados en el binario compilado
        var usados = Modulos.Ensamblados[modulo]
            .GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => Modulos.Nombres.Select(Modulos.Raiz).Contains(n))
            .Select(n => n["KillaUru.".Length..]);

        // Assert — ningún módulo usado debe estar fuera del grafo permitido (ADR-001)
        Assert.All(usados, usado =>
            Assert.Contains(usado, Modulos.GrafoPermitido[modulo]));
    }

    /// <summary>
    /// Un módulo no puede importar clases internas de otro; solo su Contracts/ es contrato público.
    /// Importar Domain o DbContext de otro módulo crea acoplamiento oculto (ADR-002).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Un_modulo_solo_usa_el_Contracts_de_otro(string modulo)
    {
        // Arrange — construir la lista de namespaces prohibidos (capas internas de otros módulos)
        var prohibidos = Modulos.Otros(modulo)
            .SelectMany(otro => CapasInternas.Select(capa => Modulos.Capa(otro, capa)))
            .ToArray();

        // Assert — ningún tipo del módulo depende de una capa interna de otro módulo (ADR-002)
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo])
                 .ShouldNot().HaveDependencyOnAny(prohibidos),
            $"{modulo}: solo puede usar el Contracts/ de otro módulo (ni su dominio ni su DbContext)");
    }

    [Fact]
    public void Shared_no_conoce_ningun_modulo()
    {
        // Arrange — referencias del ensamblado KillaUru.Shared.Domain
        var referencias = typeof(KillaUru.Shared.Domain.Roles).Assembly
            .GetReferencedAssemblies().Select(a => a.Name);

        // Assert — Shared es una librería transversal; conocer un módulo crearía un ciclo (RNF-28)
        Assert.DoesNotContain(referencias,
            n => Modulos.Nombres.Select(Modulos.Raiz).Contains(n));
    }
}
