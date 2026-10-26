using KillaUru.Anomalias.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// RN-07: el administrador solo puede mover una anomalía por las flechas de la tabla; cualquier otra
/// combinación no tiene efecto. Spec 004 · FR-025, FR-026.
/// </summary>
public sealed class ModeracionTablaTests
{
    /// <summary>
    /// Fila = estado (1 Activa … 5 Descartada); columna = acción (MarcarReparada, Descartar, Reabrir,
    /// MantenerDescartada); valor = estado destino o null si no se permite.
    /// </summary>
    private static readonly int?[,] TablaEsperada =
    {
        { 4, 5, null, null },
        { 4, 5, null, null },
        { null, 5, null, null },
        { null, null, 1, null },
        { null, null, 1, 5 },
    };

    public static TheoryData<int, int, int?> Combinaciones()
    {
        var casos = new TheoryData<int, int, int?>();
        for (var estado = 1; estado <= 5; estado++)
            for (var accion = 0; accion < 4; accion++) casos.Add(estado, accion, TablaEsperada[estado - 1, accion]);
        return casos;
    }

    /// <summary>Las 20 combinaciones estado × acción coinciden con la tabla de RN-07 (FR-025).</summary>
    [Theory, MemberData(nameof(Combinaciones))]
    public void Matriz_de_veinte_combinaciones(int estado, int accion, int? destinoEsperado)
    {
        // Act
        var destino = TablaModeracion.Destino((EstadoAnomalia)estado, (AccionAdmin)accion, porRevisar: true);

        // Assert
        Assert.Equal((EstadoAnomalia?)destinoEsperado, destino);
    }

    /// <summary>«Mantener descartada» solo tiene sentido si está «Por revisar» (FR-026).</summary>
    [Fact]
    public void Mantener_descartada_requiere_estar_por_revisar()
    {
        // Act
        var destino = TablaModeracion.Destino(EstadoAnomalia.Descartada, AccionAdmin.MantenerDescartada, porRevisar: false);

        // Assert
        Assert.Null(destino);
    }

    /// <summary>
    /// Mantener descartada reinicia el contador «Por revisar» sin crear transición, porque el estado
    /// no cambia (FR-015, FR-026).
    /// </summary>
    [Fact]
    public void Mantener_descartada_limpia_la_revision_sin_transicion()
    {
        // Arrange
        var inicial = ReportesPrueba.Crear();
        var anomalia = ReportesPrueba.Nacida(inicial);
        Assert.True(anomalia.Moderar(AccionAdmin.Descartar, ReportesPrueba.Ahora, new(), Guid.NewGuid()));
        ReportesPrueba.TresVehiculosMas(anomalia, inicial);
        var transicionesAntes = anomalia.Transiciones.Count;

        // Act
        var aplicada = anomalia.Moderar(AccionAdmin.MantenerDescartada, ReportesPrueba.Ahora.AddHours(1), new(), Guid.NewGuid());

        // Assert
        Assert.True(aplicada);
        Assert.False(anomalia.PorRevisar);
        Assert.Empty(anomalia.MarcasRevision);
        Assert.Equal(transicionesAntes, anomalia.Transiciones.Count);
    }

    /// <summary>Mantener sobre una Descartada que aún no está «Por revisar» se rechaza (FR-026).</summary>
    [Fact]
    public void Mantener_sin_revision_pendiente_se_rechaza()
    {
        // Arrange
        var anomalia = ReportesPrueba.Nacida(ReportesPrueba.Crear());
        Assert.True(anomalia.Moderar(AccionAdmin.Descartar, ReportesPrueba.Ahora, new(), Guid.NewGuid()));

        // Act
        var aplicada = anomalia.Moderar(AccionAdmin.MantenerDescartada, ReportesPrueba.Ahora, new(), Guid.NewGuid());

        // Assert
        Assert.False(aplicada);
    }
}
