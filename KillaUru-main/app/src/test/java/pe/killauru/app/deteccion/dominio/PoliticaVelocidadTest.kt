package pe.killauru.app.deteccion.dominio

import java.time.Duration
import java.time.Instant
import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T036 / RN-03: superar el umbral durante toda la ventana sostenida. */
class PoliticaVelocidadTest {
    @Test
    fun `activa a diez segundos y no antes`() {
        // Arrange
        val politica = PoliticaVelocidad(5.0, Duration.ofSeconds(10))
        val inicio = Instant.parse("2026-10-22T00:00:00Z")
        politica.registrar(6.0, inicio)
        // Act
        val antes = politica.registrar(6.0, inicio.plusSeconds(9))
        val limite = politica.registrar(6.0, inicio.plusSeconds(10))
        // Assert
        assertFalse(antes)
        assertTrue(limite)
    }

    @Test
    fun `igualar el umbral reinicia el conteo`() {
        // Arrange
        val politica = PoliticaVelocidad(5.0, Duration.ofSeconds(10))
        val inicio = Instant.parse("2026-10-22T00:00:00Z")
        politica.registrar(6.0, inicio)
        // Act
        val igual = politica.registrar(5.0, inicio.plusSeconds(10))
        val reanudado = politica.registrar(6.0, inicio.plusSeconds(11))
        // Assert
        assertFalse(igual)
        assertFalse(reanudado)
    }

    @Test
    fun `velocidad ausente o no finita impide activar`() {
        // Arrange
        val politica = PoliticaVelocidad(5.0, Duration.ofSeconds(10))
        val inicio = Instant.parse("2026-10-22T00:00:00Z")
        // Act
        val resultados = listOf(null, Double.NaN, Double.POSITIVE_INFINITY).map {
            politica.registrar(it, inicio)
        }
        // Assert
        assertTrue(resultados.none { it })
    }
}
