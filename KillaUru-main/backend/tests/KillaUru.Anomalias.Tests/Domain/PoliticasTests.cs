using KillaUru.Anomalias.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// Políticas de RN-06, RN-12 y RN-15: confirmación por dos vehículos, velocidad imposible, tope por
/// hora y persistencia por días locales. Spec 004 · FR-004, FR-010, FR-011, FR-013, FR-016.
/// </summary>
public sealed class PoliticasTests
{
    private const double MetrosPorSegundoAKmh = 3.6;

    /// <summary>
    /// Dos vehículos confirman si sus detecciones están separadas ≥ 3 min (179 s no, 180 s sí); una
    /// grave además necesita un reporte con sesión; una leve creada por tope también exige confirmar.
    /// </summary>
    [Theory]
    [InlineData(179, true, false, 4, (int)EstadoAnomalia.PendienteConfirmacion)]
    [InlineData(180, true, false, 4, (int)EstadoAnomalia.Activa)]
    [InlineData(180, false, false, 5, (int)EstadoAnomalia.PendienteConfirmacion)]
    [InlineData(180, false, true, 2, (int)EstadoAnomalia.Activa)]
    [InlineData(0, true, false, 4, (int)EstadoAnomalia.PendienteConfirmacion)]
    public void Confirmacion_por_dos_vehiculos(int segundosEntreVehiculos, bool primeroConSesion, bool porTope, int severidad, int estadoEsperado)
    {
        // Arrange
        var primero = ReportesPrueba.Crear(severidad, sesion: primeroConSesion);
        var segundo = ReportesPrueba.Crear(2, segundosEntreVehiculos);

        // Act
        var estado = new PoliticaConfirmacion(new()).Decidir([primero, segundo], primero.Severidad, porTope, segundo.DetectadoUtc);

        // Assert
        Assert.Equal((EstadoAnomalia)estadoEsperado, estado);
    }

    /// <summary>Dos celulares con el mismo seudónimo son el mismo vehículo: no confirman (FR-011).</summary>
    [Fact]
    public void Mismo_seudonimo_no_confirma()
    {
        // Arrange
        var primero = ReportesPrueba.Crear(5, sesion: true);
        var mismoVehiculo = ReportesPrueba.Crear(5, 180) with { Seudonimo = primero.Seudonimo };

        // Act
        var estado = new PoliticaConfirmacion(new()).Decidir([primero, mismoVehiculo], primero.Severidad, false, mismoVehiculo.DetectadoUtc);

        // Assert
        Assert.Equal(EstadoAnomalia.PendienteConfirmacion, estado);
    }

    /// <summary>
    /// Un vehículo es «nuevo» solo con seudónimo distinto; un seudónimo ya purgado (null) no cuenta,
    /// porque no se puede demostrar que sea otro vehículo (FR-011, FR-020).
    /// </summary>
    [Fact]
    public void Vehiculo_nuevo_requiere_seudonimo_distinto_y_vigente()
    {
        // Arrange
        var politica = new PoliticaConfirmacion(new());
        var primero = ReportesPrueba.Crear(5, sesion: true);
        var mismo = ReportesPrueba.Crear(5, 180) with { Seudonimo = primero.Seudonimo };

        // Act
        var mismoSeudonimo = politica.EsVehiculoNuevo(mismo, [primero, mismo]);
        var seudonimoPurgado = politica.EsVehiculoNuevo(mismo with { Seudonimo = null }, []);
        var otroSeudonimo = politica.EsVehiculoNuevo(mismo with { Seudonimo = Guid.NewGuid() }, [primero]);

        // Assert
        Assert.False(mismoSeudonimo);
        Assert.False(seudonimoPurgado);
        Assert.True(otroSeudonimo);
    }

    private static (ReporteAnomalia Anterior, ReporteAnomalia Actual) DesplazamientoA(double kmh, int segundos)
    {
        var anterior = ReportesPrueba.Crear();
        var grados = ReportesPrueba.GradosDe(kmh / MetrosPorSegundoAKmh * segundos);
        var actual = anterior with
        {
            EventoId = Guid.NewGuid(), DetectadoUtc = anterior.DetectadoUtc.AddSeconds(segundos), Posicion = ReportesPrueba.Punto(0, grados),
        };
        return (anterior, actual);
    }

    /// <summary>
    /// 150 km/h es el máximo plausible inclusive; con menos de 10 s entre detecciones no se evalúa,
    /// porque el error del GPS dominaría la velocidad (FR-004).
    /// </summary>
    [Theory]
    [InlineData(9, 160, null)]
    [InlineData(10, 150, null)]
    [InlineData(10, 151, (int)MotivoDescarte.VelocidadImposible)]
    public void Velocidad_maxima_plausible(int segundos, double kmh, int? motivoEsperado)
    {
        // Arrange
        var (anterior, actual) = DesplazamientoA(kmh, segundos);

        // Act
        var motivo = new PoliticaPlausibilidad(new()).Evaluar(actual, anterior);

        // Assert
        Assert.Equal((MotivoDescarte?)motivoEsperado, motivo);
    }

    /// <summary>La precisión GPS de ambos puntos se descuenta antes de estimar la velocidad (FR-004).</summary>
    [Fact]
    public void Precision_gps_se_descuenta_de_la_distancia()
    {
        // Arrange
        var (anterior, actual) = DesplazamientoA(151, 10);

        // Act
        var motivo = new PoliticaPlausibilidad(new()).Evaluar(actual with { PrecisionMetros = 20 }, anterior with { PrecisionMetros = 20 });

        // Assert
        Assert.Null(motivo);
    }

    /// <summary>Sin evento anterior del mismo seudónimo no hay velocidad que evaluar (FR-004).</summary>
    [Fact]
    public void Sin_evento_anterior_no_se_evalua()
    {
        // Act
        var motivo = new PoliticaPlausibilidad(new()).Evaluar(ReportesPrueba.Crear(), anterior: null);

        // Assert
        Assert.Null(motivo);
    }

    /// <summary>El tope es de 120 eventos por hora de detección; 121 lo supera (FR-013).</summary>
    [Theory]
    [InlineData(120, false)]
    [InlineData(121, true)]
    public void Tope_por_hora_de_deteccion(int eventos, bool superaEsperado)
    {
        // Act
        var supera = new PoliticaPlausibilidad(new()).SuperaTope(eventos);

        // Assert
        Assert.Equal(superaEsperado, supera);
    }

    /// <summary>
    /// Persistente = 3 días distintos en hora de Lima dentro de 15 días; las detecciones fuera de la
    /// ventana o futuras no cuentan (FR-016).
    /// </summary>
    [Theory]
    [InlineData(new[] { 0, -1 }, (int)EstadoPersistencia.Episodica)]
    [InlineData(new[] { 0, -1, -26 }, (int)EstadoPersistencia.Persistente)]
    [InlineData(new[] { 0, -384, 24 }, (int)EstadoPersistencia.Episodica)]
    public void Persistencia_por_dias_locales(int[] horasDesdeUltima, int esperado)
    {
        // Arrange — 05:00 UTC = medianoche en Lima: una hora antes ya es otro día local
        var ultima = new DateTimeOffset(2026, 10, 24, 5, 0, 0, TimeSpan.Zero);
        var detecciones = horasDesdeUltima.Select(horas => ultima.AddHours(horas));

        // Act
        var persistencia = new ReglaPersistencia(new()).Evaluar(detecciones, ultima);

        // Assert
        Assert.Equal((EstadoPersistencia)esperado, persistencia);
    }
}
