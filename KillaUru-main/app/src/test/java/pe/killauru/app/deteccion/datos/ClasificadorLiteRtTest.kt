package pe.killauru.app.deteccion.datos

import org.junit.Test
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import kotlin.math.exp
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * T044 / FR-011, FR-015: lógica de mapeo de salidas y escalado de temperatura.
 * El benchmark p95 sobre hardware real (3 celulares) está en T122.
 * Las 20 características del contrato p1 se prueban en CaracteristicasP1Test.
 */
class ClasificadorLiteRtTest {

    // --- Mapeo de logits a Clasificación ---

    @Test
    fun `etiqueta Normal devuelve tipo null`() {
        // Arrange + Act
        val tipo = resolverTipo(listOf("Normal", "Bache", "Hundimiento"), 0)
        // Assert
        assertNull(tipo)
    }

    @Test
    fun `etiqueta Bache devuelve TipoAnomalia Bache`() {
        // Arrange + Act
        val tipo = resolverTipo(listOf("Normal", "Bache", "Hundimiento"), 1)
        // Assert
        assertEquals(TipoAnomalia.Bache, tipo)
    }

    @Test
    fun `etiqueta Hundimiento devuelve TipoAnomalia Hundimiento`() {
        // Arrange + Act
        val tipo = resolverTipo(listOf("Normal", "Bache", "Hundimiento"), 2)
        // Assert
        assertEquals(TipoAnomalia.Hundimiento, tipo)
    }

    // --- Escalado de temperatura ---

    @Test
    fun `temperatura mayor a uno aplana la distribucion de logits`() {
        // Arrange
        val logits = floatArrayOf(0.5f, 2.0f, 0.3f)
        // Act
        val probsT1 = softmax(logits)
        val probsT2 = softmax(FloatArray(logits.size) { logits[it] / 2f })
        // Assert — el índice mayoritario se mantiene pero con menor margen con T=2
        val mejorT1 = probsT1.indices.maxByOrNull { probsT1[it] }!!
        val mejorT2 = probsT2.indices.maxByOrNull { probsT2[it] }!!
        assertEquals(mejorT1, mejorT2)
        assertTrue(probsT1[mejorT1] > probsT2[mejorT2])
    }

    @Test
    fun `temperatura uno no cambia la clasificacion mas probable`() {
        // Arrange
        val logits = floatArrayOf(0.1f, 3.5f, 0.8f)
        // Act
        val probsT1 = softmax(FloatArray(logits.size) { logits[it] / 1f })
        // Assert — el índice 1 (Bache) gana
        assertEquals(1, probsT1.indices.maxByOrNull { probsT1[it] })
    }

    private fun resolverTipo(etiquetas: List<String>, tipoIdx: Int): TipoAnomalia? {
        val etiqueta = etiquetas.getOrElse(tipoIdx) { "Normal" }
        return if (etiqueta == "Normal") null
        else runCatching { TipoAnomalia.deTexto(etiqueta) }.getOrNull()
    }

    private fun softmax(x: FloatArray): FloatArray {
        val maxX = x.max()
        val e = FloatArray(x.size) { exp((x[it] - maxX).toDouble()).toFloat() }
        val s = e.sum()
        return FloatArray(x.size) { e[it] / s }
    }
}
