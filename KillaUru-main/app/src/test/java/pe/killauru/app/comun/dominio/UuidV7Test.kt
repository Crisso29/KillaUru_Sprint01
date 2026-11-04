package pe.killauru.app.comun.dominio

import java.security.SecureRandom
import java.time.Instant
import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** T008 / FR-013: identidad estable y ordenada por hora GPS (R-004). */
class UuidV7Test {
    @Test
    fun `identificador tiene version variante y formato canonicos`() {
        // Arrange
        val hora = Instant.parse("2026-10-22T05:00:00Z")
        // Act
        val id = UuidV7.de(hora, SecureRandom())
        // Assert
        assertEquals(7, id.version())
        assertEquals(2, id.variant())
        assertEquals(36, id.toString().length)
    }

    @Test
    fun `los milisegundos preservan el orden cronologico`() {
        // Arrange
        val hora = Instant.parse("2026-10-22T05:00:00Z")
        val aleatorio = SecureRandom()
        // Act
        val anterior = UuidV7.de(hora, aleatorio).toString()
        val posterior = UuidV7.de(hora.plusMillis(1), aleatorio).toString()
        // Assert
        assertTrue(anterior < posterior)
    }

    @Test
    fun `diez mil eventos en el mismo milisegundo no se repiten`() {
        // Arrange
        val hora = Instant.parse("2026-10-22T05:00:00Z")
        val aleatorio = SecureRandom()
        // Act
        val ids = List(10_000) { UuidV7.de(hora, aleatorio) }
        // Assert
        assertEquals(ids.size, ids.toSet().size)
    }
}
