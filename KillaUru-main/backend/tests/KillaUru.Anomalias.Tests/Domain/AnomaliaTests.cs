using KillaUru.Anomalias.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// El agregado nace con su posición y kilometraje fijos, calcula su severidad y vencimiento con cada
/// reporte y nunca retrocede en el tiempo por reportes tardíos.
/// Spec 004 · FR-008, FR-009, FR-010, FR-012, FR-017, FR-019, FR-020, FR-032.
/// </summary>
public sealed class AnomaliaTests
{
    /// <summary>
    /// Leve sin tope nace Activa; grave (4–5) o creada por superar el tope nace pendiente de un
    /// segundo vehículo (FR-010). En ambos casos vence 72 h después de la detección (FR-017).
    /// </summary>
    [Theory]
    [InlineData(2, false, (int)EstadoAnomalia.Activa)]
    [InlineData(5, false, (int)EstadoAnomalia.PendienteConfirmacion)]
    [InlineData(2, true, (int)EstadoAnomalia.PendienteConfirmacion)]
    public void Nace_activa_o_pendiente_segun_severidad_y_tope(int severidad, bool porTope, int estadoEsperado)
    {
        // Arrange
        var primero = ReportesPrueba.Crear(severidad) with { KmVia = 22.5f };

        // Act
        var anomalia = ReportesPrueba.Nacida(primero, porTope: porTope);

        // Assert
        Assert.Equal((EstadoAnomalia)estadoEsperado, anomalia.Estado);
        Assert.Equal(primero.DetectadoUtc.AddHours(72), anomalia.VenceUtc);
        Assert.Equal(22.5f, anomalia.KmVia);
    }

    /// <summary>Nacer es una transición desde «nada» (Desde nulo) y se registra una sola vez (FR-015).</summary>
    [Fact]
    public void Nacer_registra_una_transicion_sin_estado_previo()
    {
        // Act
        var anomalia = ReportesPrueba.Nacida(ReportesPrueba.Crear());

        // Assert
        var transicion = Assert.Single(anomalia.Transiciones);
        Assert.Null(transicion.Desde);
    }

    /// <summary>Una Activa no vuelve a pendiente aunque llegue un reporte grave (FR-012).</summary>
    [Fact]
    public void Activa_no_regresa_a_pendiente_por_un_reporte_grave()
    {
        // Arrange
        var primero = ReportesPrueba.Crear(3);
        var anomalia = ReportesPrueba.Nacida(primero);
        var grave = ReportesPrueba.Crear(4, segundos: -60);

        // Act
        anomalia.RegistrarReporte(grave, [primero, grave], new(), ReportesPrueba.Ahora);

        // Assert
        Assert.Equal(EstadoAnomalia.Activa, anomalia.Estado);
        Assert.Equal(4, anomalia.SeveridadMostrada.Valor);
        Assert.Single(anomalia.Transiciones);
    }

    /// <summary>
    /// Un reporte detectado antes que el último no retrasa la última detección, y el kilometraje
    /// se fija al nacer: los reportes posteriores no lo cambian (FR-032).
    /// </summary>
    [Fact]
    public void Reporte_tardio_no_retrocede_la_ultima_deteccion_ni_mueve_el_km()
    {
        // Arrange
        var primero = ReportesPrueba.Crear();
        var anomalia = ReportesPrueba.Nacida(primero);
        var tardio = ReportesPrueba.Crear(segundos: -60) with { KmVia = 99 };

        // Act
        anomalia.RegistrarReporte(tardio, [primero, tardio], new(), ReportesPrueba.Ahora);

        // Assert
        Assert.Equal(primero.DetectadoUtc, anomalia.UltimaDeteccionUtc);
        Assert.Equal(0, anomalia.KmVia);
    }

    /// <summary>Una Expirada no renace con un reporte detectado justo en su vencimiento (límite exclusivo, FR-019).</summary>
    [Fact]
    public void Expirada_no_renace_con_deteccion_en_el_vencimiento()
    {
        // Arrange
        var primero = ReportesPrueba.Crear();
        var anomalia = ReportesPrueba.Nacida(primero);
        var vence = anomalia.VenceUtc;
        anomalia.Caducar(vence);

        // Act
        anomalia.RegistrarReporte(primero, [primero], new(), vence);

        // Assert
        Assert.Equal(EstadoAnomalia.Expirada, anomalia.Estado);
    }

    /// <summary>Un segundo después del vencimiento, un reporte grave la hace renacer pendiente (FR-019).</summary>
    [Fact]
    public void Expirada_renace_con_deteccion_posterior_al_vencimiento()
    {
        // Arrange
        var anomalia = ReportesPrueba.Nacida(ReportesPrueba.Crear());
        var vence = anomalia.VenceUtc;
        anomalia.Caducar(vence);
        var posterior = ReportesPrueba.Crear(5) with { DetectadoUtc = vence.AddSeconds(1) };

        // Act
        anomalia.RegistrarReporte(posterior, [posterior], new(), vence.AddSeconds(1));

        // Assert
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, anomalia.Estado);
        Assert.Equal(3, anomalia.Transiciones.Count);
    }

    /// <summary>Una Reparada renace si alguien detecta el bache después de la reparación (FR-019).</summary>
    [Fact]
    public void Reparada_renace_con_deteccion_posterior_a_la_reparacion()
    {
        // Arrange
        var anomalia = ReportesPrueba.Nacida(ReportesPrueba.Crear());
        var reparadaEn = ReportesPrueba.Ahora.AddHours(1);
        Assert.True(anomalia.Moderar(AccionAdmin.MarcarReparada, reparadaEn, new(), Guid.NewGuid()));
        var posterior = ReportesPrueba.Crear(5) with { DetectadoUtc = reparadaEn.AddSeconds(1) };

        // Act
        anomalia.RegistrarReporte(posterior, [posterior], new(), posterior.DetectadoUtc);

        // Assert
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, anomalia.Estado);
    }

    /// <summary>
    /// Una Descartada absorbe reportes sin reactivarse (su última detección avanza); tres vehículos
    /// nuevos la marcan «Por revisar» (FR-020).
    /// </summary>
    [Fact]
    public void Descartada_absorbe_reportes_y_tres_vehiculos_la_marcan_por_revisar()
    {
        // Arrange
        var inicial = ReportesPrueba.Crear();
        var anomalia = ReportesPrueba.Nacida(inicial);
        Assert.True(anomalia.Moderar(AccionAdmin.Descartar, ReportesPrueba.Ahora, new(), Guid.NewGuid()));

        // Act
        ReportesPrueba.TresVehiculosMas(anomalia, inicial);

        // Assert
        Assert.Equal(EstadoAnomalia.Descartada, anomalia.Estado);
        Assert.True(anomalia.PorRevisar);
        Assert.Equal(ReportesPrueba.Ahora.AddMinutes(9), anomalia.UltimaDeteccionUtc);
    }
}
