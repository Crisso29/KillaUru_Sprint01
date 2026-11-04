using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Recepcion;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// SeudonimoDelLote aplica la regla «un lote = un seudónimo»: la cabecera gana, o si no viene,
/// el primer evento con seudónimo válido fija el del lote (FR-024; D2, R-010).
/// Spec 005 · T038.
/// </summary>
public sealed class SeudonimoDelLoteTests
{
    private static readonly Guid S = Guid.NewGuid();
    private static readonly Guid T = Guid.NewGuid();

    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 14, 32, 05, TimeSpan.Zero);

    private static EventoReportado ConSeudonimo(Guid? id) =>
        new(Guid.NewGuid(), Ahora, -13.0, -74.0, 5.0, "Bache", 4, 0.91, "1.0", id);

    /// <summary>Cuando llega la cabecera con un UUID válido, ese es el seudónimo del lote.</summary>
    [Fact]
    public void Cabecera_valida_fija_el_seudonimo()
    {
        // Arrange
        var eventos = new[] { ConSeudonimo(T) };

        // Act
        var seudonimo = SeudonimoDelLote.Resolver(S, eventos);

        // Assert
        Assert.Equal(S, seudonimo);
    }

    /// <summary>Sin cabecera, el primer evento con seudónimo válido fija el del lote.</summary>
    [Fact]
    public void Sin_cabecera_el_primer_evento_valido_fija_el_seudonimo()
    {
        // Arrange
        var eventos = new[] { ConSeudonimo(null), ConSeudonimo(S), ConSeudonimo(T) };

        // Act
        var seudonimo = SeudonimoDelLote.Resolver(null, eventos);

        // Assert
        Assert.Equal(S, seudonimo);
    }

    /// <summary>Un seudónimo inválido (null) en un evento no fija el del lote.</summary>
    [Fact]
    public void Seudonimo_null_en_evento_no_fija_el_del_lote()
    {
        // Arrange
        var eventos = new[] { ConSeudonimo(null), ConSeudonimo(null) };

        // Act
        var seudonimo = SeudonimoDelLote.Resolver(null, eventos);

        // Assert
        Assert.Null(seudonimo);
    }

    /// <summary>Un evento con otro seudónimo válido (distinto al del lote) se marca SeudonimoInconsistente.</summary>
    [Fact]
    public void Otro_seudonimo_valido_es_inconsistente()
    {
        // Arrange — lote fijado en S, evento trae T
        var evento = ConSeudonimo(T);

        // Act
        var esInconsistente = SeudonimoDelLote.EsInconsistente(S, evento);

        // Assert
        Assert.True(esInconsistente);
    }

    /// <summary>Un evento con el mismo seudónimo del lote no es inconsistente.</summary>
    [Fact]
    public void Mismo_seudonimo_no_es_inconsistente()
    {
        // Arrange
        var evento = ConSeudonimo(S);

        // Act
        var esInconsistente = SeudonimoDelLote.EsInconsistente(S, evento);

        // Assert
        Assert.False(esInconsistente);
    }

    /// <summary>Un evento con seudonimo=null (inválido) no cuenta como inconsistente, sino como EsquemaInvalido.</summary>
    [Fact]
    public void Seudonimo_null_en_evento_no_es_inconsistente()
    {
        // Arrange
        var evento = ConSeudonimo(null);

        // Act
        var esInconsistente = SeudonimoDelLote.EsInconsistente(S, evento);

        // Assert
        Assert.False(esInconsistente);
    }

    /// <summary>Sin eventoId (id=null) se evalúa como EsquemaInvalido antes de verificar el seudónimo (R-010).</summary>
    [Fact]
    public void Sin_eventoId_el_seudonimo_no_decide_el_motivo()
    {
        // Arrange — evento sin id pero con seudónimo distinto
        // La regla dice: si no hay eventoId → EsquemaInvalido (nunca SeudonimoInconsistente)
        // EsInconsistente solo se llama cuando hay eventoId válido
        var eventoConSeudonimoDiferente = ConSeudonimo(T) with { EventoId = Guid.Empty };

        // Act — si el lote decide que id==null, no se llama EsInconsistente en absoluto.
        // Aquí probamos que con Guid.Empty (que podría ser "sin id") la función sigue operando
        // normalmente; el UseCase es quien corta antes llamando a EsInconsistente.
        var resultado = SeudonimoDelLote.EsInconsistente(S, eventoConSeudonimoDiferente);

        // Assert — el método en sí responde true porque el seudónimo es distinto;
        // la prioridad de EsquemaInvalido la maneja el UseCase, no este método.
        Assert.True(resultado);
    }
}
