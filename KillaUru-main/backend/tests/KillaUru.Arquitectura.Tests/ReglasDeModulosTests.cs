using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Monolito modular (ADR-001, ADR-002, RNF-28): un módulo usa a otro solo por su Contracts/,
/// el grafo de proyectos es exactamente el de la regla 4 y Shared no conoce a ningún módulo.
/// </summary>
public sealed class ReglasDeModulosTests
{
    private static readonly string[] CapasInternas = ["Domain", "Application", "Adapters", "Infrastructure"];

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void El_csproj_referencia_exactamente_los_modulos_del_grafo(string modulo)
    {
        var declarados = Repositorio.ModulosReferenciados(modulo).Where(Modulos.Nombres.Contains).Order();

        Assert.Equal(Modulos.GrafoPermitido[modulo].Order(), declarados);
    }

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void El_ensamblado_compilado_no_usa_modulos_fuera_del_grafo(string modulo)
    {
        var usados = Modulos.Ensamblados[modulo].GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => Modulos.Nombres.Select(Modulos.Raiz).Contains(n))
            .Select(n => n["KillaUru.".Length..]);

        Assert.All(usados, usado => Assert.Contains(usado, Modulos.GrafoPermitido[modulo]));
    }

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Un_modulo_solo_usa_el_Contracts_de_otro(string modulo)
    {
        var prohibidos = Modulos.Otros(modulo)
            .SelectMany(otro => CapasInternas.Select(capa => Modulos.Capa(otro, capa)))
            .ToArray();

        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo]).ShouldNot().HaveDependencyOnAny(prohibidos),
            $"{modulo}: solo puede usar el Contracts/ de otro módulo (ni su dominio ni su DbContext)");
    }

    [Fact]
    public void Shared_no_conoce_ningun_modulo()
    {
        var referencias = typeof(KillaUru.Shared.Domain.Roles).Assembly.GetReferencedAssemblies().Select(a => a.Name);

        Assert.DoesNotContain(referencias, n => Modulos.Nombres.Select(Modulos.Raiz).Contains(n));
    }
}
