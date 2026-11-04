package pe.killauru.app.deteccion.dominio

import org.junit.Test
import java.time.Duration
import java.time.Instant
import kotlin.test.assertNotNull
import kotlin.test.assertNull

/** T042 / FR-018, R-019: inicio, continuación y fin del viaje. */
class DetectorViajeTest {

    private var idCounter = 0
    private val detector = DetectorViaje(
        tiempoParadoMaximo = Duration.ofMinutes(30),
        generarId = { "viaje-${++idCounter}" },
    )

    @Test
    fun `en movimiento inicia viaje`() {
        // Arrange
        val t0 = Instant.parse("2026-01-01T10:00:00Z")
        // Act
        val viaje = detector.registrar(enMovimiento = true, ahora = t0, kmActual = 100.0)
        // Assert
        assertNotNull(viaje)
    }

    @Test
    fun `29 minutos detenido no termina el viaje`() {
        // Arrange
        val t0 = Instant.parse("2026-01-01T10:00:00Z")
        detector.registrar(true, t0, 100.0)
        val t1 = t0.plus(Duration.ofMinutes(29))
        // Act
        val viaje = detector.registrar(false, t1, 100.0)
        // Assert
        assertNotNull(viaje)
    }

    @Test
    fun `30 minutos detenido termina el viaje`() {
        // Arrange
        val t0 = Instant.parse("2026-01-01T10:00:00Z")
        detector.registrar(true, t0, 100.0)
        val t1 = t0.plus(Duration.ofMinutes(30))
        // Act
        detector.registrar(false, t1, 100.0)
        // Assert - después del tiempo, el viaje debería estar terminado
        val t2 = t1.plusSeconds(1)
        val viaje = detector.registrar(false, t2, 100.0)
        assertNull(viaje)
    }

    @Test
    fun `terminar servicio cierra el viaje`() {
        // Arrange
        val t0 = Instant.parse("2026-01-01T10:00:00Z")
        detector.registrar(true, t0, 100.0)
        // Act
        val viajeFinalizado = detector.terminar()
        // Assert
        assertNotNull(viajeFinalizado)
        assertNull(detector.viaje)
    }
}
