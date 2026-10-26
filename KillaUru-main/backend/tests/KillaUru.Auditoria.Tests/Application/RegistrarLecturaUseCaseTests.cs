using KillaUru.Auditoria.Application;
using KillaUru.Auditoria.Application.Puertos;
using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Auditoria.Tests.Application;

/// <summary>
/// Una lectura sensible crea su propia transacción para que el rastro se confirme antes de
/// devolver datos; si la persistencia falla, la métrica de riesgo nunca se emite (CHK016).
/// Spec 002 · FR-012, FR-017 · CHK015, CHK016 · T019, T023, T036, T037.
/// </summary>
public sealed class RegistrarLecturaUseCaseTests
{
    /// <summary>
    /// La exportación solo emite la métrica de riesgo después de confirmar la persistencia:
    /// si el repositorio falla, la métrica no llega a Grafana (CHK016, patrón AlConfirmar).
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exportacion_solo_emite_tras_confirmar(bool falla)
    {
        // Arrange
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var metricas = Substitute.For<IMetricas>();
        repositorio.AgregarConTransaccionPropiaAsync(Arg.Any<RegistroAuditoria>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                Assert.Empty(metricas.ReceivedCalls()); // la métrica no existe aún durante la inserción
                return falla ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
            });
        var caso = new RegistrarLecturaUseCase(repositorio, metricas, new FakeTimeProvider());

        // Act
        Task Ejecutar() => caso.EjecutarAsync(AccionAuditada.DatasetExportado, ActorAuditoria.Sistema,
            null, new Dictionary<string, object?>(), TestContext.Current.CancellationToken);
        if (falla) await Assert.ThrowsAsync<InvalidOperationException>(Ejecutar);
        else await Ejecutar();

        // Assert — 1 vez si hubo éxito, 0 si la persistencia falló
        metricas.Received(falla ? 0 : 1).RegistrarAccionRiesgo("DatasetExportado");
    }

    /// <summary>
    /// Un contexto con claves prohibidas (como "correo") se rechaza sin intentar insertar:
    /// la validación ocurre en el dominio, antes de tocar el repositorio (FR-001, CHK009).
    /// </summary>
    [Fact]
    public async Task Contexto_prohibido_no_llega_al_repositorio()
    {
        // Arrange
        var repositorio = Substitute.For<IRegistroAuditoriaRepositorio>();
        var caso = new RegistrarLecturaUseCase(repositorio, Substitute.For<IMetricas>(), new FakeTimeProvider());
        var contextoConDatoPrivado = new Dictionary<string, object?> { ["correo"] = "privado" };

        // Act + Assert — la excepción ocurre antes de llamar al repositorio
        await Assert.ThrowsAsync<InvalidOperationException>(() => caso.EjecutarAsync(
            AccionAuditada.AccesoAdministrativoDatos, ActorAuditoria.Sistema, null,
            contextoConDatoPrivado, TestContext.Current.CancellationToken));
        Assert.Empty(repositorio.ReceivedCalls());
    }
}
