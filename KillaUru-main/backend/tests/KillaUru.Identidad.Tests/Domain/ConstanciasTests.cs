using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// Constancias que exige la ley: la aceptación de cada documento legal y la revisión mensual
/// de privilegios. Spec 003 · FR-002, FR-004, FR-034, FR-039.
/// </summary>
public sealed class ConstanciasTests
{
    /// <summary>Se guarda qué versión aceptó y el día local, no la hora exacta (FR-002, FR-039).</summary>
    [Fact]
    public void Consentimiento_guarda_version_y_dia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var consentimiento = Consentimiento.Crear(usuarioId, DocumentoLegal.PoliticaPrivacidad, "1.2", Instantes.Ahora);

        // Assert
        Assert.Equal(usuarioId, consentimiento.UsuarioId);
        Assert.Equal(DocumentoLegal.PoliticaPrivacidad, consentimiento.Documento);
        Assert.Equal("1.2", consentimiento.Version);
        Assert.Equal(HorasGuardadas.AlDia(Instantes.Ahora), consentimiento.AceptadoUtc);
    }

    /// <summary>La revisión guarda solo totales; los ids revisados quedan en Auditoría (FR-034).</summary>
    [Fact]
    public void Revision_guarda_actor_y_totales()
    {
        // Arrange
        var actorId = Guid.NewGuid();

        // Act
        var revision = RevisionPrivilegios.Crear(actorId, 2, 1, Instantes.Ahora);

        // Assert
        Assert.Equal(0, revision.Id);
        Assert.Equal(actorId, revision.ActorId);
        Assert.Equal(2, revision.Confirmados);
        Assert.Equal(1, revision.Retirados);
        Assert.Equal(Instantes.Ahora, revision.RealizadaUtc);
    }

    /// <summary>Sin revisión previa, la primera ya está vencida: el panel la pide de inmediato (FR-034).</summary>
    [Fact]
    public void Sin_revision_previa_vence_ahora()
    {
        // Act
        var vence = RevisionPrivilegios.Vence(ultima: null, Instantes.Ahora);

        // Assert
        Assert.Equal(Instantes.Ahora, vence);
    }

    /// <summary>La siguiente revisión vence el día 1 del mes siguiente, hora de Lima (FR-034).</summary>
    [Fact]
    public void Revision_vence_el_primero_del_mes_siguiente_en_Lima()
    {
        // Arrange
        var medianocheLima1Nov = new DateTimeOffset(2026, 11, 1, 5, 0, 0, TimeSpan.Zero);

        // Act
        var vence = RevisionPrivilegios.Vence(Instantes.Ahora, Instantes.Ahora);

        // Assert
        Assert.Equal(medianocheLima1Nov, vence);
    }
}
