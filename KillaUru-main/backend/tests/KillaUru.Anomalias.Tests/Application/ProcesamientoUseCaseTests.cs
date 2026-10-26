using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Domain;
using Microsoft.Extensions.Options;
using NSubstitute;
using MotivoContrato = KillaUru.Anomalias.Contracts.MotivoDescarte;

namespace KillaUru.Anomalias.Tests.Application;

/// <summary>
/// Procesamiento de un reporte: la calidad se juzga contra la recepción original y todo efecto
/// externo (evento al mapa, métrica del tope) espera al commit. Spec 004 · FR-001, FR-002, FR-013, FR-014, FR-031.
/// </summary>
public sealed class ProcesamientoUseCaseTests
{
    private static EvaluarIntrinsecaUseCase Evaluacion() =>
        new(EscenarioProcesamiento.Corredor(), Options.Create(new AnomaliasOptions()));

    /// <summary>
    /// La antigüedad se mide contra la recepción, no contra el reloj del procesamiento: un evento de
    /// exactamente 7 días se acepta, uno segundo más tarde ya no (FR-002).
    /// </summary>
    [Theory]
    [InlineData(0, null)]
    [InlineData(1, (int)MotivoContrato.TimestampFueraDeRango)]
    public void Antiguedad_se_mide_contra_la_recepcion(int segundosDespuesDeSieteDias, int? motivoEsperado)
    {
        // Arrange
        var recibido = EscenarioProcesamiento.Ahora;
        var evento = EscenarioProcesamiento.Evento(recibido.AddDays(-7));

        // Act
        var motivo = Evaluacion().Ejecutar(evento, recibido.AddSeconds(segundosDespuesDeSieteDias));

        // Assert
        Assert.Equal((MotivoContrato?)motivoEsperado, motivo);
    }

    /// <summary>La evaluación es pura: un campo ausente se informa como EsquemaInvalido, sin excepción (FR-001).</summary>
    [Fact]
    public void Campo_ausente_es_esquema_invalido()
    {
        // Arrange
        var evento = EscenarioProcesamiento.Evento(EscenarioProcesamiento.Ahora) with { ModeloVersion = null };

        // Act
        var motivo = Evaluacion().Ejecutar(evento, EscenarioProcesamiento.Ahora);

        // Assert
        Assert.Equal(MotivoContrato.EsquemaInvalido, motivo);
    }

    /// <summary>Con commit correcto, el mapa se entera y la métrica del tope sube, ambas después del commit (FR-014).</summary>
    [Fact]
    public async Task Commit_correcto_publica_y_cuenta_el_tope_despues()
    {
        // Arrange
        var escenario = new EscenarioProcesamiento();

        // Act
        var resultado = await escenario.ProcesarConTopeSuperadoAsync();

        // Assert
        Assert.Null(resultado.Motivo);
        Assert.False(escenario.HuboEfectosAntesDelCommit);
        Assert.Equal(1, escenario.Publicaciones);
        escenario.Metricas.Received(1).RegistrarRetenidoPorTope();
    }

    /// <summary>Si el commit falla, nada sale del módulo: ni evento al mapa ni métrica (FR-014).</summary>
    [Fact]
    public async Task Commit_fallido_no_publica_ni_cuenta_el_tope()
    {
        // Arrange
        var escenario = new EscenarioProcesamiento(commitFalla: true);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(escenario.ProcesarConTopeSuperadoAsync);
        Assert.Equal(0, escenario.Publicaciones);
        escenario.Metricas.DidNotReceive().RegistrarRetenidoPorTope();
    }

    /// <summary>Un evento leve que supera el tope por hora crea la anomalía pendiente de confirmación (FR-013).</summary>
    [Fact]
    public async Task Tope_superado_crea_la_anomalia_pendiente()
    {
        // Arrange
        var escenario = new EscenarioProcesamiento();

        // Act
        await escenario.ProcesarConTopeSuperadoAsync();

        // Assert
        await escenario.Anomalias.Received(1).GuardarAsync(
            Arg.Is<Anomalia>(a => a.Estado == EstadoAnomalia.PendienteConfirmacion), true, Arg.Any<CancellationToken>());
    }

    /// <summary>La duración de cada evento se mide siempre, también si el commit falla (FR-031).</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Duracion_del_proceso_se_registra_siempre(bool commitFalla)
    {
        // Arrange
        var escenario = new EscenarioProcesamiento(commitFalla);

        // Act
        await Record.ExceptionAsync(escenario.ProcesarConTopeSuperadoAsync);

        // Assert
        escenario.Metricas.Received(1).RegistrarDuracionProceso(Arg.Any<TimeSpan>());
    }
}
