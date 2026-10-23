using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// El dominio es el centro: no conoce frameworks, ni las otras capas de su módulo,
/// ni los contratos de otro módulo (constitución II, D4).
/// </summary>
/// <remarks>
/// También prohíben Contracts propio y los servicios técnicos de Shared (tabla de enfoque §3).
/// </remarks>
public sealed class ReglasDeDominioTests
{
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Domain_no_usa_frameworks(string modulo) =>
        Regla.Cumple(
            DelDominio(modulo).ShouldNot().HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore"),
            $"{modulo}: Domain no depende de EF Core ni de ASP.NET");

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Domain_no_usa_las_capas_externas_de_su_modulo(string modulo) =>
        Regla.Cumple(
            DelDominio(modulo).ShouldNot().HaveDependencyOnAny(
                Modulos.Capa(modulo, "Application"), Modulos.Capa(modulo, "Adapters"), Modulos.Capa(modulo, "Infrastructure"),
                Modulos.Capa(modulo, "Contracts"), "KillaUru.Shared.Eventos", "KillaUru.Shared.Persistencia",
                "KillaUru.Shared.Metricas", "KillaUru.Shared.Tareas"),
            $"{modulo}: Domain no depende de Application, Adapters ni Infrastructure");

    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Domain_no_usa_otros_modulos(string modulo) =>
        Regla.Cumple(
            DelDominio(modulo).ShouldNot().HaveDependencyOnAny([.. Modulos.Otros(modulo).Select(Modulos.Raiz)]),
            $"{modulo}: Domain no usa tipos de otro módulo, ni siquiera su Contracts/");

    private static PredicateList DelDominio(string modulo) =>
        Types.InAssembly(Modulos.Ensamblados[modulo]).That().ResideInNamespace(Modulos.Capa(modulo, "Domain"));
}
