package pe.killauru.app.deteccion.dominio

import org.junit.Test
import java.time.Instant
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T040 / FR-011, R-018: varianza bajo umbral no pasa al modelo. */
class PrefiltroTest {

    private val prefiltro = Prefiltro(varianzaMinima = 0.5)

    private fun ventanaPlana(): Ventana {
        val muestras = List(100) { floatArrayOf(0f, 0f, 9.8f) }
        return Ventana(muestras, Instant.EPOCH)
    }

    private fun ventanaConBache(): Ventana {
        val muestras = (0 until 100).map { i ->
            if (i == 50) floatArrayOf(0f, 0f, 25f) else floatArrayOf(0f, 0f, 9.8f)
        }
        return Ventana(muestras, Instant.EPOCH)
    }

    @Test
    fun `ventana plana no pasa el prefiltro`() {
        // Arrange
        val ventana = ventanaPlana()
        // Act
        val pasa = prefiltro.pasar(ventana)
        // Assert
        assertFalse(pasa)
    }

    @Test
    fun `ventana con bache pasa el prefiltro`() {
        // Arrange
        val ventana = ventanaConBache()
        // Act
        val pasa = prefiltro.pasar(ventana)
        // Assert
        assertTrue(pasa)
    }

    @Test
    fun `ventana vacia no pasa el prefiltro`() {
        // Arrange
        val ventana = Ventana(emptyList(), Instant.EPOCH)
        // Act / Assert
        assertFalse(prefiltro.pasar(ventana))
    }
}
