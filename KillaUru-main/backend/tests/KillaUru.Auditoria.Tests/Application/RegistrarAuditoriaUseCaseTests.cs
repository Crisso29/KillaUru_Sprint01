using System.Data.Common;
using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
using KillaUru.Shared.Persistencia;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Auditoria.Tests.Application;

/// <summary>La escritura exige transacción y nunca anuncia un riesgo antes de su confirmación.</summary>
public sealed class RegistrarAuditoriaUseCaseTests
{
    [Fact]
    public async Task Sin_transaccion_no_inserta()
    {
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new RegistrarAuditoriaUseCase(repositorio, Substitute.For<IContextoTransaccion>(),
            Substitute.For<IMetricas>(), new FakeTimeProvider());
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(AccionAuditada.RolCambiado,
            ActorAuditoria.Sistema, null, new Dictionary<string, object?>(), TestContext.Current.CancellationToken));
        Assert.Empty(repositorio.ReceivedCalls());
    }

    [Theory]
    [InlineData(AccionAuditada.RolCambiado, true)]
    [InlineData(AccionAuditada.CuentaBloqueada, true)]
    [InlineData(AccionAuditada.DatasetExportado, true)]
    [InlineData(AccionAuditada.UsuarioCreado, false)]
    public async Task Solo_agenda_los_riesgos_y_no_los_emite_al_insertar(AccionAuditada accion, bool riesgo)
    {
        var tx = Substitute.For<IContextoTransaccion>();
        tx.Conexion.Returns(Substitute.For<DbConnection>());
        tx.Transaccion.Returns(Substitute.For<DbTransaction>());
        var metricas = Substitute.For<IMetricas>();
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new RegistrarAuditoriaUseCase(repositorio, tx, metricas, new FakeTimeProvider());
        await caso.EjecutarAsync(accion, ActorAuditoria.Sistema, null, new Dictionary<string, object?>(), TestContext.Current.CancellationToken);
        await repositorio.Received(1).AgregarEnTransaccionAsync(Arg.Any<RegistroAuditoria>(), Arg.Any<CancellationToken>());
        tx.Received(riesgo ? 1 : 0).AlConfirmar(Arg.Any<Func<Task>>());
        Assert.Empty(metricas.ReceivedCalls());
    }
}
