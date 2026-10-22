using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Regla de dependencia hacia adentro de Clean Architecture (enfoque-arquitectonico.md §3, D4):
/// los controladores solo ven Application; Application no conoce infraestructura ni EF Core.
/// </summary>
public sealed class ReglasDeCapasTests
{
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Adapters_no_usa_Domain_ni_Infrastructure(string modulo) =>
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo]).That().ResideInNamespace(Modulos.Capa(modulo, "Adapters"))
                .ShouldNot().HaveDependencyOnAny(Modulos.Capa(modulo, "Domain"), Modulos.Capa(modulo, "Infrastructure")),
            $"{modulo}: Adapters solo puede usar Application");

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Application_no_usa_Adapters_Infrastructure_ni_EF(string modulo) =>
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo]).That().ResideInNamespace(Modulos.Capa(modulo, "Application"))
                .ShouldNot().HaveDependencyOnAny(
                    Modulos.Capa(modulo, "Adapters"), Modulos.Capa(modulo, "Infrastructure"), "Microsoft.EntityFrameworkCore"),
            $"{modulo}: Application no depende de Adapters, Infrastructure ni EF Core");

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Infrastructure_no_usa_Adapters(string modulo) =>
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo]).That().ResideInNamespace(Modulos.Capa(modulo, "Infrastructure"))
                .ShouldNot().HaveDependencyOn(Modulos.Capa(modulo, "Adapters")),
            $"{modulo}: Infrastructure no depende de Adapters");
}
