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

/// <summary>
/// La escritura de auditoría es inseparable de la transacción del llamador: sin ella no se inserta
/// y el riesgo solo se anuncia DESPUÉS de que el llamador confirme el commit (patrón AlConfirmar).
/// Spec 002 · FR-002, FR-003 · CHK007, CHK014, CHK016 · T009, T016.
/// </summary>
public sealed class RegistrarAuditoriaUseCaseTests
{
    /// <summary>
    /// Sin transacción activa el use case lanza antes de intentar cualquier inserción:
    /// previene auditar una acción que luego se descartaría por rollback (CHK007, CHK014).
    /// </summary>
    [Fact]
    public async Task Sin_transaccion_no_inserta()
    {
        // Arrange
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new RegistrarAuditoriaUseCase(repositorio, Substitute.For<IContextoTransaccion>(),
            Substitute.For<IMetricas>(), new FakeTimeProvider());

        // Act + Assert — la excepción ocurre antes de llamar al repositorio
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(AccionAuditada.RolCambiado,
            ActorAuditoria.Sistema, null, new Dictionary<string, object?>(), TestContext.Current.CancellationToken));
        Assert.Empty(repositorio.ReceivedCalls());
    }

    /// <summary>
    /// Las acciones de riesgo (RolCambiado=2, CuentaBloqueada=7, DatasetExportado=13) registran
    /// un callback en AlConfirmar; las que no son de riesgo (UsuarioCreado=1) no lo hacen.
    /// La métrica nunca se emite al insertar: se agenda para después del commit (CHK016).
    /// </summary>
    [Theory]
    [InlineData(AccionAuditada.RolCambiado, true)]
    [InlineData(AccionAuditada.CuentaBloqueada, true)]
    [InlineData(AccionAuditada.DatasetExportado, true)]
    [InlineData(AccionAuditada.UsuarioCreado, false)]
    public async Task Solo_agenda_los_riesgos_y_no_los_emite_al_insertar(AccionAuditada accion, bool riesgo)
    {
        // Arrange
        var tx = Substitute.For<IContextoTransaccion>();
        tx.Conexion.Returns(Substitute.For<DbConnection>());
        tx.Transaccion.Returns(Substitute.For<DbTransaction>());
        var metricas = Substitute.For<IMetricas>();
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new RegistrarAuditoriaUseCase(repositorio, tx, metricas, new FakeTimeProvider());

        // Act
        await caso.EjecutarAsync(accion, ActorAuditoria.Sistema, null,
            new Dictionary<string, object?>(), TestContext.Current.CancellationToken);

        // Assert
        await repositorio.Received(1).AgregarEnTransaccionAsync(
            Arg.Any<RegistroAuditoria>(), Arg.Any<CancellationToken>());
        tx.Received(riesgo ? 1 : 0).AlConfirmar(Arg.Any<Func<Task>>());
        Assert.Empty(metricas.ReceivedCalls()); // la métrica no se emite aquí, sino en AlConfirmar
    }
}
