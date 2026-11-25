package pe.killauru.app.deteccion.dominio

import org.junit.Test
import java.time.Instant
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T040 / FR-011, FR-018: v_pap < umbral no pasa al modelo; ahorra batería sin movimiento real. */
class PrefiltroTest {

    private val prefiltro = Prefiltro(umbralPicoAPicoMs2 = 1.2f)

    private fun ventana(aV: FloatArray) = Ventana(
        aV = aV,
        aH = FloatArray(25) { 0f },
        w = FloatArray(25) { 0f },
        contextoAv = FloatArray(100) { 0f },
        velocidadKmH = 0f,
        inicioUtc = Instant.EPOCH,
    )

    @Test
    fun `ventana plana no pasa el prefiltro`() {
        // Arrange
        val v = ventana(FloatArray(25) { 0f })
        // Act
        val pasa = prefiltro.pasar(v)
        // Assert
        assertFalse(pasa)
    }

    @Test
    fun `ventana con bache pasa el prefiltro`() {
        // Arrange — pico = 10, valle = 0 → v_pap = 10 >= 1.2
        val aV = FloatArray(25) { if (it == 12) 10f else 0f }
        val v = ventana(aV)
        // Act
        val pasa = prefiltro.pasar(v)
        // Assert
        assertTrue(pasa)
    }

    @Test
    fun `ventana vacia no pasa el prefiltro`() {
        // Arrange
        val v = Ventana(FloatArray(0), FloatArray(0), FloatArray(0), FloatArray(0), 0f, Instant.EPOCH)
        // Act / Assert
        assertFalse(prefiltro.pasar(v))
    }

    @Test
    fun `v_pap exactamente al umbral pasa el prefiltro`() {
        // Arrange — pap = 1.2 = umbral
        val aV = FloatArray(25) { 0f }.also { it[0] = 1.2f }
        val v = ventana(aV)
        // Act / Assert
        assertTrue(prefiltro.pasar(v))
    }
}
