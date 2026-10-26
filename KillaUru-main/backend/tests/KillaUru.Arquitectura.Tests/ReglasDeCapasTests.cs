using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>
/// Spec 001 — FR-029 (D4, CHK035, enfoque-arquitectonico.md §3):
/// reglas de dependencia de Clean Architecture: las capas externas conocen a las internas, nunca al revés.
/// Adapters → Application → Domain; Infrastructure → Application/Domain; nunca al revés.
/// Garantiza que los controladores HTTP no mezclen lógica de negocio con acceso a datos.
/// </summary>
public sealed class ReglasDeCapasTests
{
    /// <summary>
    /// Adapters (controladores HTTP) deben delegar en Application; acceder directamente a Domain
    /// o Infrastructure acoplaría el transporte HTTP a la persistencia (D4).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Adapters_no_usa_Domain_ni_Infrastructure(string modulo) =>
        // Arrange+Act+Assert en línea: NetArchTest evalúa las dependencias del ensamblado compilado
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo])
                 .That().ResideInNamespace(Modulos.Capa(modulo, "Adapters"))
                 .ShouldNot().HaveDependencyOnAny(
                     Modulos.Capa(modulo, "Domain"),
                     Modulos.Capa(modulo, "Infrastructure")),
            $"{modulo}: Adapters solo puede usar Application");

    /// <summary>
    /// Application orquesta casos de uso: no puede depender de la infraestructura porque
    /// eso impediría intercambiar EF Core por otro ORM o reemplazar el transporte (D4).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Application_no_usa_Adapters_Infrastructure_ni_EF(string modulo) =>
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo])
                 .That().ResideInNamespace(Modulos.Capa(modulo, "Application"))
                 .ShouldNot().HaveDependencyOnAny(
                     Modulos.Capa(modulo, "Adapters"),
                     Modulos.Capa(modulo, "Infrastructure"),
                     "Microsoft.EntityFrameworkCore"),
            $"{modulo}: Application no depende de Adapters, Infrastructure ni EF Core");

    /// <summary>
    /// Infrastructure implementa las interfaces de Application; si dependiera de Adapters
    /// se crearía un ciclo que impide compilar los ensamblados por separado (D4).
    /// </summary>
    [Theory]
    [MemberData(nameof(Modulos.Todos), MemberType = typeof(Modulos))]
    public void Infrastructure_no_usa_Adapters(string modulo) =>
        Regla.Cumple(
            Types.InAssembly(Modulos.Ensamblados[modulo])
                 .That().ResideInNamespace(Modulos.Capa(modulo, "Infrastructure"))
                 .ShouldNot().HaveDependencyOn(Modulos.Capa(modulo, "Adapters")),
            $"{modulo}: Infrastructure no depende de Adapters");
}
