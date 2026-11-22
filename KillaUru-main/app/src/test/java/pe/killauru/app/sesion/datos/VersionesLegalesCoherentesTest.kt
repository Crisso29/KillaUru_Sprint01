package pe.killauru.app.sesion.datos

import org.json.JSONObject
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import java.io.File
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * FR-002, FR-003, R-024: la versión legal que trae el APK es la vigente del backend. Si no coinciden, el registro desde
 * la app envía una versión vieja y el backend responde `aceptacion_requerida` (así estaba: APK 1.0, backend 1.2).
 * Patrón AAA.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class VersionesLegalesCoherentesTest {
    private val apk = JSONObject(File("src/main/assets/legal/textos.json").readText())
    private val backend = File("../backend/src/KillaUru.Api/appsettings.json").readText()

    private fun vigente(clave: String) = Regex(""""$clave"\s*:\s*"([^"]+)"""").find(backend)!!.groupValues[1]

    @Test fun `los terminos del apk son la version vigente del backend`() {
        // Arrange + Act
        val enApk = apk.getJSONObject("terminos").getString("version")
        // Assert
        assertEquals(vigente("VersionTerminos"), enApk)
    }

    @Test fun `la politica del apk es la version vigente del backend`() {
        // Arrange + Act
        val enApk = apk.getJSONObject("politica").getString("version")
        // Assert
        assertEquals(vigente("VersionPolitica"), enApk)
    }

    /** Ley 29733 y prototipo: el aviso dice a quién llegan los datos. Decir «no compartimos con terceros» sería falso. */
    @Test fun `el aviso de privacidad nombra a quienes reciben los datos`() {
        // Arrange + Act
        val texto = apk.getJSONObject("politica").getString("texto")
        // Assert
        listOf("Cloudflare", "Oracle Cloud", "Grafana Cloud").forEach { assertTrue(it in texto, "Falta $it") }
    }
}
