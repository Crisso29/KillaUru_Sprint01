using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Recepcion;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// LoteAceptado separa eventos únicos de repetidos dentro del lote y construye los parámetros
/// para el INSERT con un solo seudónimo en el payload (FR-006, FR-008, R-003).
/// Spec 005 · T005.
/// </summary>
public sealed class LoteAceptadoTests
{
    private static readonly Guid LoteId = Guid.NewGuid();
    private static readonly Guid Seudonimo = Guid.NewGuid();
    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 14, 32, 05, TimeSpan.Zero);

    private static EventoReportado Evento(Guid id) =>
        new(id, Ahora.AddMinutes(-5), -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0", Seudonimo);

    /// <summary>Eventos con distintos ids quedan todos en Aceptados y RepetidosEnLote está vacío.</summary>
    [Fact]
    public void Eventos_unicos_van_todos_a_aceptados()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var eventos = new[] { Evento(id1), Evento(id2) };

        // Act
        var lote = LoteAceptado.De(LoteId, Ahora, true, eventos, []);

        // Assert
        Assert.Equal(2, lote.Aceptados.Count);
        Assert.Empty(lote.RepetidosEnLote);
    }

    /// <summary>El mismo id dos veces en el lote: el primero a Aceptados, el segundo a RepetidosEnLote (R-003).</summary>
    [Fact]
    public void Id_repetido_en_el_lote_va_a_repetidos()
    {
        // Arrange
        var id = Guid.NewGuid();
        var eventos = new[] { Evento(id), Evento(id) };

        // Act
        var lote = LoteAceptado.De(LoteId, Ahora, true, eventos, []);

        // Assert
        Assert.Single(lote.Aceptados);
        Assert.Single(lote.RepetidosEnLote);
        Assert.Equal(id, lote.RepetidosEnLote[0]);
    }

    /// <summary>ComoParametros no incluye el seudónimo en el payload de los eventos (FR-008).</summary>
    [Fact]
    public void ComoParametros_excluye_el_seudonimo_del_payload()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lote = LoteAceptado.De(LoteId, Ahora, true, [Evento(id)], []);

        // Act
        var (_, _, _, _, payloads, _) = lote.ComoParametros();

        // Assert — el payload no debe contener "seudonimo"
        Assert.All(payloads, p => Assert.DoesNotContain("seudonimo", p.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Los LoteId, RecibidoUtc y ConSesion del lote se propagan correctamente.</summary>
    [Fact]
    public void Parametros_del_lote_se_propagan()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lote = LoteAceptado.De(LoteId, Ahora, false, [Evento(id)], []);

        // Act
        var (loteIdParam, recibidoParam, conSesionParam, _, _, _) = lote.ComoParametros();

        // Assert
        Assert.Equal(LoteId, loteIdParam);
        Assert.Equal(Ahora, recibidoParam);
        Assert.False(conSesionParam);
    }
}
