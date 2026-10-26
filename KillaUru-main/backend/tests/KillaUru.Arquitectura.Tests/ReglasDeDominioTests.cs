using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Spec 001 — FR-029 (D4, constitución II, CHK035, enfoque-arquitectonico.md §3):
/// el dominio es el núcleo puro: no conoce frameworks, ni las capas externas de su módulo,
/// ni los contratos de otro módulo. Esto garantiza que las reglas de negocio sean portables
/// y testeables sin levantar ninguna infraestructura.
/// </summary>
public sealed class ReglasDeDominioTests
{
    /// <summary>
    /// El dominio debe poder compilarse sin ASP.NET ni EF Core.
    /// Si los importa, el modelo de negocio queda atado al stack técnico (D4).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Domain_no_usa_frameworks(string modulo) =>
        // Arrange+Act+Assert: NetArchTest inspecciona el ensamblado compilado (IL)
        Regla.Cumple(
            DelDominio(modulo).ShouldNot()
                .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore"),
            $"{modulo}: Domain no depende de EF Core ni de ASP.NET");

    /// <summary>
    /// Domain no debe conocer Application, Adapters, Infrastructure, Contracts
    /// ni los servicios técnicos de Shared (Eventos, Persistencia, Metricas, Tareas).
    /// Esos son detalles de implementación que el dominio no puede anticipar (D4).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Domain_no_usa_las_capas_externas_de_su_modulo(string modulo) =>
        Regla.Cumple(
            DelDominio(modulo).ShouldNot().HaveDependencyOnAny(
                Modulos.Capa(modulo, "Application"),
                Modulos.Capa(modulo, "Adapters"),
                Modulos.Capa(modulo, "Infrastructure"),
                Modulos.Capa(modulo, "Contracts"),
                "KillaUru.Shared.Eventos",
                "KillaUru.Shared.Persistencia",
                "KillaUru.Shared.Metricas",
                "KillaUru.Shared.Tareas"),
            $"{modulo}: Domain no depende de Application, Adapters ni Infrastructure");

    /// <summary>
    /// Un módulo no debe conocer el dominio de otro; solo puede usar su Contracts/.
    /// Usar el dominio de otro módulo crea acoplamiento implícito entre contextos acotados (D4).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Domain_no_usa_otros_modulos(string modulo) =>
        Regla.Cumple(
            DelDominio(modulo).ShouldNot()
                .HaveDependencyOnAny([.. Modulos.Otros(modulo).Select(Modulos.Raiz)]),
            $"{modulo}: Domain no usa tipos de otro módulo, ni siquiera su Contracts/");

    // Filtra los tipos del namespace Domain del módulo dado para reutilizar en cada regla
    private static PredicateList DelDominio(string modulo) =>
        Types.InAssembly(Modulos.Ensamblados[modulo]).That()
             .ResideInNamespace(Modulos.Capa(modulo, "Domain"));
}
