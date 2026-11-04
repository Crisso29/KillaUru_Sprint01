package pe.killauru.app.seudonimo.dominio

import java.time.Duration
import java.time.Instant
import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse

/** T033 / RN-09: no inventar antigüedad cuando el reloj confiable no está disponible. */
class PoliticaRotacionTest {
    @Test
    fun `rota a treinta dias exactos`() {
        // Arrange
        val politica = PoliticaRotacion(Duration.ofDays(30))
        val desde = Instant.parse("2026-10-01T00:00:00Z")
        val limite = desde.plus(Duration.ofDays(30))
        // Act
        val resultados = listOf(limite.minusMillis(1), limite, limite.plusMillis(1))
            .map { politica.debeRotar(desde, it) }
        // Assert
        assertEquals(listOf(false, true, true), resultados)
    }

    @Test
    fun `sin hora confiable conserva el seudonimo`() {
        // Arrange
        val politica = PoliticaRotacion(Duration.ofDays(30))
        val desde = Instant.parse("2026-10-01T00:00:00Z")
        // Act
        val rota = politica.debeRotar(desde, null)
        // Assert
        assertFalse(rota)
    }
}
