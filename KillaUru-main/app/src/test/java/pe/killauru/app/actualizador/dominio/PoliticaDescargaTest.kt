package pe.killauru.app.actualizador.dominio

import org.junit.Test
import pe.killauru.app.mapa.dominio.PoliticaDescarga
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/**
 * T087 / FR-043, FR-044: política de descarga del mapa PMTiles.
 * Patrón AAA en cada test.
 */
class PoliticaDescargaTest {

    private val politica = PoliticaDescarga()

    @Test fun `wifi disponible y versiones distintas debe descargar`() {
        // Arrange
        val versionLocal = "v1"
        val versionRemota = "v2"
        // Act
        val resultado = politica.debeDescargar(versionLocal, versionRemota, usarDatosMoviles = false, hayWifi = true)
        // Assert
        assertTrue(resultado)
    }

    @Test fun `datos moviles sin permiso no debe descargar`() {
        // Arrange
        val versionLocal = "v1"
        val versionRemota = "v2"
        // Act
        val resultado = politica.debeDescargar(versionLocal, versionRemota, usarDatosMoviles = false, hayWifi = false)
        // Assert
        assertFalse(resultado, "sin Wi-Fi y sin permiso de datos móviles no debe descargar")
    }

    @Test fun `datos moviles con permiso debe descargar`() {
        // Arrange
        val versionLocal = "v1"
        val versionRemota = "v2"
        // Act
        val resultado = politica.debeDescargar(versionLocal, versionRemota, usarDatosMoviles = true, hayWifi = false)
        // Assert
        assertTrue(resultado)
    }

    @Test fun `misma version no debe descargar aunque haya wifi`() {
        // Arrange
        val version = "v3"
        // Act
        val resultado = politica.debeDescargar(version, version, usarDatosMoviles = false, hayWifi = true)
        // Assert
        assertFalse(resultado, "la versión es la misma; no hay nada nuevo que descargar")
    }

    @Test fun `sin version local debe descargar cuando hay wifi`() {
        // Arrange - primer arranque: nunca se descargó nada
        val versionLocal = null
        val versionRemota = "v1"
        // Act
        val resultado = politica.debeDescargar(versionLocal, versionRemota, usarDatosMoviles = false, hayWifi = true)
        // Assert
        assertTrue(resultado, "si no hay copia local siempre debe descargar con red disponible")
    }
}
