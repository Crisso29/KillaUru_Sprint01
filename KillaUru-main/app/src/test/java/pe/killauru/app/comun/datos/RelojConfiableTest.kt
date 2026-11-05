package pe.killauru.app.comun.datos

import java.time.Duration
import java.time.Instant
import org.junit.Test
import pe.killauru.app.comun.dominio.AnclaTiempo
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** T009 / FR-026, R-003: solo hora GPS/servidor propagada por monotónico, nunca hora de pared. */
class RelojConfiableTest {
    @Test
    fun `ancla gps proporciona hora confiable via monotonico`() {
        // Arrange
        val utc = Instant.parse("2026-01-01T10:00:00Z")
        val ancla = AnclaTiempo(utc, Duration.ofSeconds(100), arranque = 1)
        // Act
        val resultado = ancla.hora(Duration.ofSeconds(110), arranqueActual = 1)
        // Assert
        assertEquals(Instant.parse("2026-01-01T10:00:10Z"), resultado)
    }

    @Test
    fun `ancla del servidor proporciona hora confiable`() {
        // Arrange
        val utc = Instant.parse("2026-06-15T08:30:00Z")
        val ancla = AnclaTiempo(utc, Duration.ofSeconds(50), arranque = 2)
        // Act
        val resultado = ancla.hora(Duration.ofSeconds(70), arranqueActual = 2)
        // Assert
        assertEquals(Instant.parse("2026-06-15T08:30:20Z"), resultado)
    }

    @Test
    fun `reinicio devuelve null`() {
        // Arrange
        val ancla = AnclaTiempo(Instant.parse("2026-01-01T12:00:00Z"), Duration.ofSeconds(100), arranque = 1)
        // Act
        val resultado = ancla.hora(Duration.ofSeconds(10), arranqueActual = 2)
        // Assert
        assertNull(resultado)
    }

    @Test
    fun `mover reloj de pared no cambia el resultado porque depende solo del monotonico`() {
        // Arrange
        val utc = Instant.parse("2026-01-01T10:00:00Z")
        val ancla = AnclaTiempo(utc, Duration.ofSeconds(100), arranque = 1)
        // Act
        val r1 = ancla.hora(Duration.ofSeconds(150), arranqueActual = 1)
        val r2 = ancla.hora(Duration.ofSeconds(150), arranqueActual = 1)
        // Assert
        assertEquals(r1, r2)
        assertEquals(Instant.parse("2026-01-01T10:00:50Z"), r1)
    }

    @Test
    fun `monotonico menor que el ancla devuelve null`() {
        // Arrange
        val ancla = AnclaTiempo(Instant.parse("2026-01-01T10:00:00Z"), Duration.ofSeconds(100), arranque = 1)
        // Act
        val resultado = ancla.hora(Duration.ofSeconds(50), arranqueActual = 1)
        // Assert
        assertNull(resultado)
    }
}
