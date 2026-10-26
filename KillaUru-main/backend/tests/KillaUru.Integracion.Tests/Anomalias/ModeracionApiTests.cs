using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KillaUru.Auditoria.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Anomalias;

/// <summary>
/// La tabla de RN-07 aplicada por HTTP: cada acción permitida cambia el estado y se audita en el mismo
/// commit; las no permitidas responden 409 sin efectos. Spec 004 · FR-025, FR-027, FR-030.
/// </summary>
[Collection("Anomalias")]
public sealed class ModeracionApiTests(BaseDatosPrueba bd)
{
    private static readonly string[] Acciones = ["MarcarReparada", "Descartar", "Reabrir", "MantenerDescartada"];

    /// <summary>Fila = estado 1–5; columna = acción; valor = estado destino o null si no se permite.</summary>
    private static readonly int?[,] TablaEsperada =
    {
        { 4, 5, null, null },
        { 4, 5, null, null },
        { null, 5, null, null },
        { null, null, 1, null },
        { null, null, 1, 5 },
    };

    public static TheoryData<int, string, int?> Combinaciones()
    {
        var casos = new TheoryData<int, string, int?>();
        for (var estado = 1; estado <= 5; estado++)
            for (var accion = 0; accion < Acciones.Length; accion++) casos.Add(estado, Acciones[accion], TablaEsperada[estado - 1, accion]);
        return casos;
    }

    private sealed record Moderacion(HttpResponseMessage Respuesta, int EstadoFinal, int Auditorias);

    /// <summary>Pone la anomalía en <paramref name="estado"/>, pide la acción por HTTP y lee el resultado.</summary>
    private static async Task<Moderacion> ModerarDesdeAsync(EscenarioAnomalias escenario, int estado, string accion)
    {
        var id = await escenario.ProcesarYObtenerAnomaliaAsync(escenario.Evento());
        await escenario.EjecutarSqlAsync($"UPDATE anomalias.anomalia SET estado={(short)estado}, por_revisar={estado == 5} WHERE id={id}");
        var actor = Guid.NewGuid();
        using var cliente = escenario.ClienteComo("Administrador", actor);
        var respuesta = await cliente.PostAsJsonAsync($"/api/v1/anomalies/{id}/moderacion", new { accion }, EscenarioAnomalias.Ct);
        await using var scope = escenario.Api.Services.CreateAsyncScope();
        var auditorias = await scope.ServiceProvider.GetRequiredService<AuditoriaDbContext>().Registros
            .CountAsync(r => r.ActorId == actor && r.Objetivo == id.ToString(), EscenarioAnomalias.Ct);
        return new(respuesta, (int)(await escenario.LeerAnomaliaAsync(id)).Estado, auditorias);
    }

    /// <summary>Las 20 combinaciones: 200 y un registro de auditoría si se permite; 409 y nada si no (FR-025).</summary>
    [Theory, MemberData(nameof(Combinaciones))]
    public async Task Matriz_completa_y_auditoria(int estado, string accion, int? destino)
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);
        var permitida = destino is not null;

        // Act
        var resultado = await ModerarDesdeAsync(escenario, estado, accion);

        // Assert
        Assert.Equal(permitida ? HttpStatusCode.OK : HttpStatusCode.Conflict, resultado.Respuesta.StatusCode);
        Assert.Equal(destino ?? estado, resultado.EstadoFinal);
        Assert.Equal(permitida ? 1 : 0, resultado.Auditorias);
    }

    /// <summary>Tras reparar o descartar ya no hay vencimiento: el detalle lo muestra nulo (FR-027).</summary>
    [Theory]
    [InlineData(1, "MarcarReparada")]
    [InlineData(1, "Descartar")]
    public async Task Estado_final_no_tiene_vencimiento(int estado, string accion)
    {
        // Arrange
        using var escenario = new EscenarioAnomalias(bd);

        // Act
        var resultado = await ModerarDesdeAsync(escenario, estado, accion);

        // Assert
        using var detalle = JsonDocument.Parse(await resultado.Respuesta.Content.ReadAsStringAsync(EscenarioAnomalias.Ct));
        Assert.Equal(JsonValueKind.Null, detalle.RootElement.GetProperty("venceUtc").ValueKind);
    }
}
