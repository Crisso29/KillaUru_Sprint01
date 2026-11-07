package pe.killauru.app.sincronizacion.dominio

import java.time.Duration
import org.junit.Test
import kotlin.test.assertEquals

/** T057 / FR-035…FR-036: jitter reproducible por inyección, nunca dormir en el dominio. */
class PoliticaReintentoTest {
    @Test
    fun `el backoff respeta la secuencia y el tope`() {
        // Arrange
        val politica = PoliticaReintento(listOf(30L, 120, 600, 1800, 7200), 25)
        // Act
        val segundos = (0..5).map { politica.espera(it, 0.5).seconds }
        // Assert
        assertEquals(listOf(30L, 120, 600, 1800, 7200, 7200), segundos)
    }

    @Test
    fun `jitter ordinario puede acortar o alargar un cuarto`() {
        // Arrange
        val politica = PoliticaReintento(listOf(30L, 120, 600, 1800, 7200), 25)
        // Act
        val minimo = politica.espera(0, 0.0)
        val maximo = politica.espera(0, 1.0)
        // Assert
        assertEquals(Duration.ofMillis(22_500), minimo)
        assertEquals(Duration.ofMillis(37_500), maximo)
    }

    @Test
    fun `retry after nunca se acorta`() {
        // Arrange
        val politica = PoliticaReintento(listOf(30L, 120, 600, 1800, 7200), 25)
        // Act
        val minimo = politica.esperaRetryAfter(Duration.ofSeconds(60), 0.0)
        val maximo = politica.esperaRetryAfter(Duration.ofSeconds(60), 1.0)
        // Assert
        assertEquals(Duration.ofSeconds(60), minimo)
        assertEquals(Duration.ofSeconds(75), maximo)
    }
}
