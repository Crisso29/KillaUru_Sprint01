package pe.killauru.app.actualizador.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T118 / FR-070, FR-071, R-017: política de activación del modelo. */
class PoliticaActivacionModeloTest {
    private val politica = PoliticaActivacionModelo(versionApp = "1.2.0")

    @Test
    fun `version mayor compatible se puede descargar`() {
        // Arrange
        val manifiesto = ManifiestoModelo(
            version = "0.2.0", archivo = "modelo.tflite", esquema = 1,
            etiquetas = listOf("Normal", "Bache", "Hundimiento"), severidades = 5,
            versionMinimaApp = "1.0.0",
        )
        // Act / Assert
        assertTrue(politica.puedeActivar(manifiesto))
    }

    @Test
    fun `versionMinimaApp mayor que app instalada no activa`() {
        // Arrange
        val manifiesto = ManifiestoModelo(
            version = "0.3.0", archivo = "modelo.tflite", esquema = 1,
            etiquetas = listOf("Normal", "Bache", "Hundimiento"), severidades = 5,
            versionMinimaApp = "2.0.0",
        )
        // Act / Assert
        assertFalse(politica.puedeActivar(manifiesto))
    }

    @Test
    fun `esquema 2 no es compatible`() {
        // Arrange
        val manifiesto = ManifiestoModelo(
            version = "0.2.0", archivo = "modelo.tflite", esquema = 2,
            etiquetas = listOf("Normal", "Bache", "Hundimiento"), severidades = 5,
            versionMinimaApp = "1.0.0",
        )
        // Act / Assert
        assertFalse(politica.puedeActivar(manifiesto))
    }

    @Test
    fun `etiquetas distintas no activa`() {
        // Arrange
        val manifiesto = ManifiestoModelo(
            version = "0.2.0", archivo = "modelo.tflite", esquema = 1,
            etiquetas = listOf("Normal", "Grieta"),
            severidades = 5, versionMinimaApp = "1.0.0",
        )
        // Act / Assert
        assertFalse(politica.puedeActivar(manifiesto, etiquetasEsperadas = listOf("Normal", "Bache")))
    }
}
