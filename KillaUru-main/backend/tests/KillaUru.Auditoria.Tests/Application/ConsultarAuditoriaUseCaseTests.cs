using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Auditoria.Tests.Application;

/// <summary>
/// Toda consulta al log de auditoría queda a su vez registrada: el acceso a datos sensibles
/// es trazable (autorregistro, CHK015). Los filtros inválidos se rechazan antes de cualquier I/O.
/// Spec 002 · FR-011, FR-017 · CHK001, CHK015, CHK018 · T020, T025, T036, T037.
/// </summary>
public sealed class ConsultarAuditoriaUseCaseTests
{
    /// <summary>
    /// Un tamaño de página fuera del rango permitido (1–200) devuelve null sin consultar
    /// ni registrar el acceso: la validación es la primera línea de defensa (FR-011).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task Rechaza_tamano_sin_consultar(int tamano)
    {
        // Arrange
        var consulta = Substitute.For<IConsultaAuditoria>();
        var escritura = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new ConsultarAuditoriaUseCase(consulta,
            new(escritura, Substitute.For<IMetricas>(), new FakeTimeProvider()));

        // Act
        var resultado = await caso.EjecutarAsync(new(Tamano: tamano), Guid.NewGuid(),
            "Administrador", TestContext.Current.CancellationToken);

        // Assert — ningún I/O ocurrió
        Assert.Null(resultado);
        Assert.Empty(consulta.ReceivedCalls());
        Assert.Empty(escritura.ReceivedCalls());
    }

    /// <summary>
    /// Una consulta válida devuelve la página y deja un rastro de acceso que incluye
    /// los filtros usados; el registro de acceso ocurre DESPUÉS de leer (CHK015).
    /// </summary>
    [Fact]
    public async Task Aplica_filtros_y_registra_el_acceso()
    {
        // Arrange
        var filtros = new FiltrosAuditoria(Categoria: "Sesion", Tamano: 20);
        var consulta = Substitute.For<IConsultaAuditoria>();
        consulta.ConsultarAsync(filtros, Arg.Any<CancellationToken>()).Returns(new PaginaAuditoria([], 1, 20, 0));
        var escritura = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new ConsultarAuditoriaUseCase(consulta,
            new(escritura, Substitute.For<IMetricas>(), new FakeTimeProvider()));

        // Act
        var resultado = await caso.EjecutarAsync(filtros, Guid.NewGuid(),
            "Administrador", TestContext.Current.CancellationToken);

        // Assert — se devuelve la página y se audita el acceso con los filtros aplicados
        Assert.NotNull(resultado);
        await escritura.Received(1).AgregarConTransaccionPropiaAsync(
            Arg.Is<RegistroAuditoria>(r => r.Accion == 14 && r.Contexto.Contains("Sesion")),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Varias combinaciones de filtros inválidos se detectan antes de ir a la base de datos:
    /// fechas invertidas, categoría o acción fuera del catálogo, número de página imposible.
    /// </summary>
    [Fact]
    public void Rechaza_fechas_invertidas_y_catalogos_invalidos()
    {
        // Arrange
        var ahora = new FakeTimeProvider().GetUtcNow();

        // Act + Assert — cada filtro inválido se rechaza de forma independiente
        Assert.False(new FiltrosAuditoria(ahora, ahora.AddSeconds(-1)).EsValido);   // fin antes que inicio
        Assert.False(new FiltrosAuditoria(Categoria: "otra").EsValido);             // categoría inexistente
        Assert.False(new FiltrosAuditoria(Accion: "99").EsValido);                  // código fuera del catálogo
        Assert.False(new FiltrosAuditoria(Pagina: int.MaxValue).EsValido);          // número de página imposible
        Assert.True(new FiltrosAuditoria(ahora, ahora, Tamano: 200).EsValido);      // mismo instante: válido
    }
}
