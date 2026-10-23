using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Auditoria.Tests.Application;

/// <summary>El riesgo se emite después de la persistencia confirmada, nunca si esta falla.</summary>
public sealed class RegistrarLecturaUseCaseTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Exportacion_solo_emite_tras_confirmar(bool falla)
    {
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var metricas = Substitute.For<IMetricas>();
        repositorio.AgregarConTransaccionPropiaAsync(Arg.Any<RegistroAuditoria>(), Arg.Any<CancellationToken>())
            .Returns(_ => { Assert.Empty(metricas.ReceivedCalls()); return falla ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask; });
        var caso = new RegistrarLecturaUseCase(repositorio, metricas, new FakeTimeProvider());
        Task Ejecutar() => caso.EjecutarAsync(AccionAuditada.DatasetExportado, ActorAuditoria.Sistema,
            null, new Dictionary<string, object?>(), TestContext.Current.CancellationToken);
        if (falla) await Assert.ThrowsAsync<InvalidOperationException>(Ejecutar);
        else await Ejecutar();
        metricas.Received(falla ? 0 : 1).RegistrarAccionRiesgo("DatasetExportado");
    }

    [Fact]
    public async Task Contexto_prohibido_no_llega_al_repositorio()
    {
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new RegistrarLecturaUseCase(repositorio, Substitute.For<IMetricas>(), new FakeTimeProvider());
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(AccionAuditada.AccesoAdministrativoDatos,
            ActorAuditoria.Sistema, null, new Dictionary<string, object?> { ["correo"] = "privado" }, TestContext.Current.CancellationToken));
        Assert.Empty(repositorio.ReceivedCalls());
    }
}
