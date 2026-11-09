package pe.killauru.app.deteccion.dominio

import org.junit.Test
import java.time.Instant
import kotlin.test.assertEquals

/** T040 / FR-014, R-018: fusión de positivos cercanos. */
class FusionVentanasTest {

    private val fusion = FusionVentanas(ventanaMsFusion = 1_000L)

    private fun ventana(offsetMs: Long) =
        Ventana(listOf(floatArrayOf(0f, 0f, 1f)), Instant.EPOCH.plusMillis(offsetMs))

    private fun clasif(c: Float) = Clasificacion(TipoAnomalia.Bache, 3, c)

    @Test
    fun `dos positivos a 500 ms se fusionan en uno`() {
        // Arrange
        val pares = listOf(
            ventana(0) to clasif(0.80f),
            ventana(500) to clasif(0.90f),
        )
        // Act
        val result = fusion.fusionar(pares)
        // Assert
        assertEquals(1, result.size)
        assertEquals(0.90f, result[0].second.confidence)
    }

    @Test
    fun `dos positivos a 1001 ms dan dos eventos`() {
        // Arrange
        val pares = listOf(
            ventana(0) to clasif(0.80f),
            ventana(1001) to clasif(0.80f),
        )
        // Act
        val result = fusion.fusionar(pares)
        // Assert
        assertEquals(2, result.size)
    }

    @Test
    fun `dos positivos exactamente a 1000 ms se fusionan`() {
        // Arrange
        val pares = listOf(
            ventana(0) to clasif(0.80f),
            ventana(1000) to clasif(0.85f),
        )
        // Act
        val result = fusion.fusionar(pares)
        // Assert
        assertEquals(1, result.size)
    }

    @Test
    fun `clasificaciones normales se ignorar`() {
        // Arrange
        val pares = listOf(ventana(0) to Clasificacion(null, 1, 0.99f))
        // Act
        val result = fusion.fusionar(pares)
        // Assert
        assertEquals(0, result.size)
    }
}
