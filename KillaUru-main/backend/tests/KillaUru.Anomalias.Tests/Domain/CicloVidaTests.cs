using KillaUru.Anomalias.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// Plazos del ciclo de vida y unicidad del historial: cada cambio de estado deja exactamente una
/// transición. Spec 004 · FR-015, FR-016, FR-017, FR-019.
/// </summary>
public sealed class CicloVidaTests
{
    /// <summary>Detectada en 3 días distintos dentro de 15, es persistente y su Activa vence a 7 días (FR-016, FR-017).</summary>
    [Fact]
    public void Persistente_activa_vence_en_siete_dias()
    {
        // Arrange
        var hoy = ReportesPrueba.Crear();
        var anomalia = Anomalia.Crear(hoy, false, ReportesPrueba.Ahora);
        ReporteAnomalia[] tresDias = [hoy, hoy with { DetectadoUtc = hoy.DetectadoUtc.AddDays(-1) }, hoy with { DetectadoUtc = hoy.DetectadoUtc.AddDays(-2) }];

        // Act
        anomalia.RegistrarReporte(hoy, tresDias, new(), ReportesPrueba.Ahora);

        // Assert
        Assert.Equal(EstadoPersistencia.Persistente, anomalia.Persistencia);
        Assert.Equal(hoy.DetectadoUtc.AddDays(7), anomalia.VenceUtc);
        Assert.Single(anomalia.Transiciones);
    }

    /// <summary>Confirmar una grave con un segundo vehículo a 3 min agrega una sola transición Pendiente → Activa (FR-015).</summary>
    [Fact]
    public void Confirmacion_agrega_una_sola_transicion()
    {
        // Arrange
        var primero = ReportesPrueba.Crear(5, sesion: true);
        var anomalia = ReportesPrueba.Nacida(primero);
        var segundoVehiculo = ReportesPrueba.Crear(5, segundos: 180);

        // Act
        anomalia.RegistrarReporte(segundoVehiculo, [primero, segundoVehiculo], new(), segundoVehiculo.DetectadoUtc);

        // Assert
        Assert.Equal(2, anomalia.Transiciones.Count);
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, anomalia.Transiciones[1].Desde);
        Assert.Equal(EstadoAnomalia.Activa, anomalia.Transiciones[1].Hacia);
    }

    /// <summary>Una leve expirada renace directamente Activa, con una sola transición Expirada → Activa (FR-019).</summary>
    [Fact]
    public void Leve_expirada_renace_directamente_activa()
    {
        // Arrange
        var primero = ReportesPrueba.Crear();
        var anomalia = ReportesPrueba.Nacida(primero);
        anomalia.Caducar(anomalia.VenceUtc);
        var posterior = primero with { EventoId = Guid.NewGuid(), DetectadoUtc = anomalia.VenceUtc.AddSeconds(1) };

        // Act
        anomalia.RegistrarReporte(posterior, [posterior], new(), posterior.DetectadoUtc);

        // Assert
        Assert.Equal(3, anomalia.Transiciones.Count);
        Assert.Equal(EstadoAnomalia.Expirada, anomalia.Transiciones[2].Desde);
        Assert.Equal(EstadoAnomalia.Activa, anomalia.Transiciones[2].Hacia);
    }
}
