package pe.killauru.grabador

import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * T015 / FR-004, FR-005 — Metadatos: campos requeridos, sin identificadores del aparato,
 * categoría solo Liviano (RN-18).
 */
class MetadatosTest {

    private fun metadatosValidos() = Metadatos(
        grabacionId = "20261026-R01-C1",
        recorrido = "R01",
        grabadorVersion = "1.0.0",
        celularAlias = "C1",
        celularMarcaModelo = "Samsung Galaxy A34",
        celularRamGb = 4,
        celularAnioLanzamiento = 2023,
        celularAndroid = "14",
        vehiculoAlias = "V1",
        soporteTipo = "rejilla",
        soportePosicion = "centro-tablero",
        tramoKmInicio = 268.0,
        tramoKmFin = 335.073,
        tramoSentido = "AyacuchoVinchos",
        fecha = "2026-10-26",
        esCelularVideo = false,
    )

    @Test
    fun `meta json no contiene imei`() {
        // Arrange
        val meta = metadatosValidos()
        // Act
        val json = meta.toJson()
        // Assert — FR-005: sin IMEI
        assertFalse(json.contains("imei", ignoreCase = true))
    }

    @Test
    fun `meta json no contiene android id`() {
        // Arrange
        val meta = metadatosValidos()
        // Act
        val json = meta.toJson()
        // Assert
        assertFalse(json.contains("android_id", ignoreCase = true))
    }

    @Test
    fun `meta json no contiene placa`() {
        // Arrange
        val meta = metadatosValidos()
        // Act
        val json = meta.toJson()
        // Assert
        assertFalse(json.contains("placa", ignoreCase = true))
    }

    @Test
    fun `categoria siempre es Liviano`() {
        // Arrange
        val meta = metadatosValidos()
        // Act
        val json = meta.toJson()
        // Assert — RN-18, S-06
        assertTrue(json.contains("\"categoria\":\"Liviano\"") || json.contains("\"categoria\": \"Liviano\""))
    }

    @Test
    fun `grabacion id sigue el formato esperado`() {
        // Arrange
        val meta = metadatosValidos()
        // Assert — formato AAAAMMDD-Rxx-Cx
        assertTrue(meta.grabacionId.matches(Regex("""\d{8}-R\d{2}-C\d""")))
    }

    @Test
    fun `es celular video false por defecto`() {
        // Arrange
        val meta = metadatosValidos()
        // Assert
        assertFalse(meta.esCelularVideo)
    }
}
