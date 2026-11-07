package pe.killauru.app.comun.dominio

import java.time.Duration
import java.time.Instant
import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** T009 / R-003: cálculo puro utilizado por el adaptador de reloj confiable. */
class AnclaTiempoTest {
    @Test
    fun `avanza solo con el tiempo monotonico del mismo arranque`() {
        // Arrange
        val utc = Instant.parse("2026-10-22T10:00:00Z")
        val ancla = AnclaTiempo(utc, Duration.ofSeconds(100), 7)
        // Act
        val ahora = ancla.hora(Duration.ofSeconds(105), 7)
        // Assert
        assertEquals(utc.plusSeconds(5), ahora)
    }

    @Test
    fun `reiniciar o retroceder el monotonico invalida el ancla`() {
        // Arrange
        val ancla = AnclaTiempo(Instant.EPOCH, Duration.ofSeconds(100), 7)
        // Act
        val reiniciado = ancla.hora(Duration.ofSeconds(105), 8)
        val retrocedido = ancla.hora(Duration.ofSeconds(99), 7)
        // Assert
        assertNull(reiniciado)
        assertNull(retrocedido)
    }
}
