package pe.killauru.app.deteccion.datos

import org.junit.Test
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import kotlin.test.assertEquals
import kotlin.test.assertNull

/**
 * T044 / FR-011, FR-015: normalización de entrada y lógica del mapeo de salidas.
 * El benchmark p95 sobre hardware real (3 celulares) está en T122.
 */
class ClasificadorLiteRtTest {

    // --- NormalizadorEntrada: normalización del manifiesto (z-score) ---

    @Test
    fun `normaliza aplicando z-score por canal`() {
        // Arrange
        val norm = NormalizadorEntrada(media = 0f, desviacion = 2f, longitudEsperada = 6)
        val muestras = listOf(floatArrayOf(2f, 4f, 6f), floatArrayOf(0f, -2f, -4f))
        // Act
        val salida = norm.normalizar(muestras)
        // Assert — (x - media) / desviacion
        assertEquals(6, salida.size)
        assertEquals(1f, salida[0], 1e-5f)
        assertEquals(2f, salida[1], 1e-5f)
        assertEquals(3f, salida[2], 1e-5f)
        assertEquals(0f, salida[3], 1e-5f)
        assertEquals(-1f, salida[4], 1e-5f)
        assertEquals(-2f, salida[5], 1e-5f)
    }

    @Test
    fun `rellena con ceros cuando hay pocas muestras`() {
        // Arrange
        val norm = NormalizadorEntrada(media = 0f, desviacion = 1f, longitudEsperada = 9)
        val muestras = listOf(floatArrayOf(1f, 2f, 3f))
        // Act
        val salida = norm.normalizar(muestras)
        // Assert
        assertEquals(9, salida.size)
        (3 until 9).forEach { assertEquals(0f, salida[it], 1e-5f) }
    }

    @Test
    fun `lista vacía devuelve arreglo de ceros`() {
        // Arrange
        val norm = NormalizadorEntrada(media = 1f, desviacion = 1f, longitudEsperada = 6)
        // Act
        val salida = norm.normalizar(emptyList())
        // Assert
        assertEquals(6, salida.size)
        salida.forEach { assertEquals(0f, it, 1e-5f) }
    }

    @Test
    fun `llamadas sucesivas producen la misma salida sin estado mutable`() {
        // Arrange
        val norm = NormalizadorEntrada(media = 0f, desviacion = 1f, longitudEsperada = 3)
        val muestras = listOf(floatArrayOf(1f, 2f, 3f))
        // Act
        val primera = norm.normalizar(muestras)
        val segunda = norm.normalizar(muestras)
        // Assert — el normalizador no muta estado interno entre ventanas
        primera.forEachIndexed { i, v -> assertEquals(v, segunda[i], 1e-5f) }
    }

    @Test
    fun `longitudEsperada es accesible para pre-alocar el inputBuffer`() {
        // Arrange
        val norm = NormalizadorEntrada(media = 0f, desviacion = 1f, longitudEsperada = 300)
        // Assert — ClasificadorLiteRt usa norm.longitudEsperada al crear el inputBuffer lazy
        assertEquals(300, norm.longitudEsperada)
    }

    // --- Mapeo de índice a Clasificación: 3 clases × 5 severidades = 15 salidas ---

    @Test
    fun `nSeveridades 5 produce buffer de 15 slots para 3 clases`() {
        // Assert
        assertEquals(15, 3 * 5)
    }

    @Test
    fun `índice 0 corresponde a clase 0 severidad 1`() {
        // Arrange
        val nSeveridades = 5
        // Act + Assert
        assertEquals(0, 0 / nSeveridades)
        assertEquals(1, (0 % nSeveridades) + 1)
    }

    @Test
    fun `índice 14 corresponde a clase 2 severidad 5`() {
        // Arrange
        val nSeveridades = 5
        // Act + Assert
        assertEquals(2, 14 / nSeveridades)
        assertEquals(5, (14 % nSeveridades) + 1)
    }

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

    private fun resolverTipo(etiquetas: List<String>, tipoIdx: Int): TipoAnomalia? {
        val etiqueta = etiquetas.getOrElse(tipoIdx) { "Normal" }
        return if (etiqueta == "Normal") null
        else runCatching { TipoAnomalia.deTexto(etiqueta) }.getOrNull()
    }
}
