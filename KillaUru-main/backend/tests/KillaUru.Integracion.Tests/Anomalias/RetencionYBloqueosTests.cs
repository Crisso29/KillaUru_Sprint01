using KillaUru.Anomalias.Application.Tareas;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using EstadoAnomalia = KillaUru.Anomalias.Domain.EstadoAnomalia;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// Retención de 2 años con límites exactos, purga de seudónimos por hora de recepción y tareas que
/// saltan filas bloqueadas sin esperarlas. Spec 004 · FR-018, FR-021, FR-022.
/// </summary>
[Collection("Anomalias")]
public sealed class RetencionYBloqueosTests(BaseDatosPrueba bd)
{
    /// <summary>Simula un trabajador que tiene la fila con FOR UPDATE en otra conexión.</summary>
    private sealed class FilaBloqueada(AsyncServiceScope scope, IDbContextTransaction transaccion) : IAsyncDisposable
    {
        internal static async Task<FilaBloqueada> TomarAsync(EscenarioAnomalias escenario, Guid id)
        {
            var scope = escenario.Api.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
            var transaccion = await db.Database.BeginTransactionAsync(EscenarioAnomalias.Ct);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM anomalias.anomalia WHERE id={id} FOR UPDATE", EscenarioAnomalias.Ct);
            return new(scope, transaccion);
        }

        internal Task LiberarAsync() => transaccion.RollbackAsync(EscenarioAnomalias.Ct);

        public async ValueTask DisposeAsync()
        {
            await transaccion.DisposeAsync();
            await scope.DisposeAsync();
        }
    }

    /// <summary>
    /// Una fila bloqueada por el procesamiento no frena la caducidad (SKIP LOCKED): se salta y se
    /// caduca en la siguiente pasada (FR-018).
    /// </summary>
    [Fact]
    public async Task Caducidad_salta_una_fila_bloqueada_y_la_recupera_despues()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        escenario.Reloj.Advance(TimeSpan.FromHours(73));
        await using var bloqueo = await FilaBloqueada.TomarAsync(escenario, id);
        await using var trabajador = escenario.Api.Services.CreateAsyncScope();
        var tarea = trabajador.ServiceProvider.GetRequiredService<CaducarAnomaliasUseCase>();

        // Act + Assert — con la fila bloqueada no espera ni la caduca
        await tarea.EjecutarAsync(EscenarioAnomalias.Ct).WaitAsync(TimeSpan.FromSeconds(10), EscenarioAnomalias.Ct);
        Assert.Equal(EstadoAnomalia.Activa, (await escenario.LeerAnomaliaAsync(id)).Estado);

        // Act + Assert — liberada, la siguiente pasada la caduca
        await bloqueo.LiberarAsync();
        await tarea.EjecutarAsync(EscenarioAnomalias.Ct);
        Assert.Equal(EstadoAnomalia.Expirada, (await escenario.LeerAnomaliaAsync(id)).Estado);
    }

    /// <summary>
    /// Se borran Expiradas y Reparadas cerradas hace más de 730 días (límite exacto: 730 días no, un
    /// segundo más sí). Una Descartada se conserva mientras siga recibiendo detecciones (FR-022).
    /// </summary>
    [Theory]
    [InlineData((int)EstadoAnomalia.Expirada, 0, 800, false)]
    [InlineData((int)EstadoAnomalia.Expirada, 1, 800, true)]
    [InlineData((int)EstadoAnomalia.Reparada, 1, 800, true)]
    [InlineData((int)EstadoAnomalia.Descartada, 1, 365, false)]
    [InlineData((int)EstadoAnomalia.Descartada, 1, 800, true)]
    public async Task Historico_respeta_el_limite_y_la_actividad_de_descartadas(int estado, int segundosPasadoElLimite, int diasDesdeUltimaDeteccion, bool eliminadaEsperada)
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        var cerradaDesde = escenario.Reloj.GetUtcNow().AddDays(-730).AddSeconds(-segundosPasadoElLimite);
        var ultimaDeteccion = escenario.Reloj.GetUtcNow().AddDays(-diasDesdeUltimaDeteccion);
        await escenario.EjecutarSqlAsync(
            $"UPDATE anomalias.anomalia SET estado={(short)estado}, estado_desde_utc={cerradaDesde}, ultima_deteccion_utc={ultimaDeteccion} WHERE id={id}");
        await using var scope = escenario.Api.Services.CreateAsyncScope();

        // Act
        await scope.ServiceProvider.GetRequiredService<PurgarHistoricoUseCase>().EjecutarAsync(EscenarioAnomalias.Ct);

        // Assert
        var sigue = await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Anomalias.AnyAsync(a => a.Id == id, EscenarioAnomalias.Ct);
        Assert.Equal(!eliminadaEsperada, sigue);
    }

    /// <summary>
    /// La purga de seudónimos cuenta 72 h desde la recepción, no desde la detección: un evento detectado
    /// hace 6 días pero recibido hace 1 h conserva su seudónimo y su hora exacta (FR-021).
    /// </summary>
    [Fact]
    public async Task Deteccion_antigua_recibida_recientemente_conserva_seudonimo_y_hora()
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var evento = escenario.Evento() with { DetectadoUtc = escenario.Reloj.GetUtcNow().AddDays(-6) };
        await escenario.ProcesarAsync(evento);
        var recibido = escenario.Reloj.GetUtcNow();
        escenario.Reloj.Advance(TimeSpan.FromHours(1));
        await using var scope = escenario.Api.Services.CreateAsyncScope();

        // Act
        await scope.ServiceProvider.GetRequiredService<PurgarSeudonimosUseCase>().EjecutarAsync(EscenarioAnomalias.Ct);

        // Assert
        var reporte = await scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>().Reportes.SingleAsync(r => r.EventoId == evento.EventoId, EscenarioAnomalias.Ct);
        Assert.Equal(evento.Seudonimo, reporte.Seudonimo);
        Assert.Equal(recibido, reporte.RecibidoUtc);
    }
}
