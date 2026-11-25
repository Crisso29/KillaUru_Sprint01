package pe.killauru.app.actualizador.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T118 / FR-070, FR-071, R-017: política de activación del modelo. */
class PoliticaActivacionModeloTest {
    private val politica = PoliticaActivacionModelo(versionApp = "1.2.0")

    private fun manifiesto(version: String = "0.2.0", versionMinima: String = "1.0.0",
                           clases: List<String> = listOf("Normal", "Bache", "Hundimiento")) =
        ManifiestoModelo(version, "modelo.tflite", clases, versionMinima, 1.0f, 1.2f)

    @Test
    fun `version mayor compatible se puede descargar`() {
        // Arrange / Act / Assert
        assertTrue(politica.puedeActivar(manifiesto()))
    }

    @Test
    fun `versionMinimaApp mayor que app instalada no activa`() {
        // Arrange / Act / Assert
        assertFalse(politica.puedeActivar(manifiesto(versionMinima = "2.0.0")))
    }

    @Test
    fun `clases distintas no activan el modelo`() {
        // Arrange
        val m = manifiesto(clases = listOf("Normal", "Grieta"))
        // Act / Assert
        assertFalse(politica.puedeActivar(m, clasesEsperadas = listOf("Normal", "Bache")))
    }

    @Test
    fun `clases correctas y version compatible activan el modelo`() {
        // Arrange / Act / Assert
        assertTrue(politica.puedeActivar(manifiesto(version = "1.0.0")))
    }
}
