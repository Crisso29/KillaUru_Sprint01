using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Auditoria.Tests.Application;

/// <summary>Valida antes del I/O y audita exactamente los filtros de una consulta válida.</summary>
public sealed class ConsultarAuditoriaUseCaseTests
{
    [Theory]
    [InlineData(0)] [InlineData(201)]
    public async Task Rechaza_tamano_sin_consultar(int tamano)
    {
        var consulta = Substitute.For<IConsultaAuditoria>();
        var escritura = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new ConsultarAuditoriaUseCase(consulta,
            new(escritura, Substitute.For<IMetricas>(), new FakeTimeProvider()));
        Assert.Null(await caso.EjecutarAsync(new(Tamano: tamano), Guid.NewGuid(), "Administrador", TestContext.Current.CancellationToken));
        Assert.Empty(consulta.ReceivedCalls());
        Assert.Empty(escritura.ReceivedCalls());
    }

    [Fact]
    public async Task Aplica_filtros_y_registra_el_acceso()
    {
        var filtros = new FiltrosAuditoria(Categoria: "Sesion", Tamano: 20);
        var consulta = Substitute.For<IConsultaAuditoria>();
        consulta.ConsultarAsync(filtros, Arg.Any<CancellationToken>()).Returns(new PaginaAuditoria([], 1, 20, 0));
        var escritura = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new ConsultarAuditoriaUseCase(consulta, new(escritura, Substitute.For<IMetricas>(), new FakeTimeProvider()));
        Assert.NotNull(await caso.EjecutarAsync(filtros, Guid.NewGuid(), "Administrador", TestContext.Current.CancellationToken));
        await escritura.Received(1).AgregarConTransaccionPropiaAsync(
            Arg.Is<RegistroAuditoria>(r => r.Accion == 14 && r.Contexto.Contains("Sesion")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Rechaza_fechas_invertidas_y_catalogos_invalidos()
    {
        var ahora = new FakeTimeProvider().GetUtcNow();
        Assert.False(new FiltrosAuditoria(ahora, ahora.AddSeconds(-1)).EsValido);
        Assert.False(new FiltrosAuditoria(Categoria: "otra").EsValido);
        Assert.False(new FiltrosAuditoria(Accion: "99").EsValido);
        Assert.False(new FiltrosAuditoria(Pagina: int.MaxValue).EsValido);
        Assert.True(new FiltrosAuditoria(ahora, ahora, Tamano: 200).EsValido);
    }
}
