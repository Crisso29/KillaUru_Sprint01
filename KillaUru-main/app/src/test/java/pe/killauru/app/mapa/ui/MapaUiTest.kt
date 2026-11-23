package pe.killauru.app.mapa.ui

import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import pe.killauru.app.actualizador.ui.PantallaDescargaUi
import pe.killauru.app.mapa.dominio.EstadoDescarga
import kotlin.test.assertEquals

/**
 * T089 / FR-049, FR-052: tests de UI para los componentes de la pantalla de mapa.
 * Patrón AAA en cada test. Robolectric para Compose en JVM.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class MapaUiTest {

    @get:Rule val composeTestRule = createComposeRule()

    // ── SinMapaPantalla ──────────────────────────────────────────────────────

    /** Prototipo 13: dice cuánto pesa, que la detección sigue y deja descargar ya o con datos móviles. */
    @Test fun `sin mapa explica el peso y deja descargar ahora o con datos moviles`() {
        // Arrange
        var descargar = 0
        var moviles = 0
        composeTestRule.setContent {
            SinMapaPantalla(PantallaDescargaUi(EstadoDescarga.EsperandoWifi(2_362_855)), { descargar++ }, { moviles++ })
        }
        // Act
        composeTestRule.onNodeWithText("Descargar ahora").performClick()
        composeTestRule.onNodeWithText("Usar datos móviles").performClick()
        // Assert
        composeTestRule.onNodeWithText("Aún no tienes el mapa").assertIsDisplayed()
        composeTestRule.onNodeWithText("Conéctate a Wi-Fi para descargarlo (2,3 MB)", substring = true).assertIsDisplayed()
        composeTestRule.onNodeWithText("la detección sigue guardando", substring = true).assertIsDisplayed()
        assertEquals(1 to 1, descargar to moviles)
    }

    @Test fun `mientras descarga muestra el avance en vez del boton`() {
        // Arrange + Act
        composeTestRule.setContent { SinMapaPantalla(PantallaDescargaUi(EstadoDescarga.Descargando(40, 2_362_855)), {}, {}) }
        // Assert
        composeTestRule.onNodeWithText("Descargando… 40 %").assertIsDisplayed()
        composeTestRule.onNodeWithText("Descargar ahora").assertDoesNotExist()
    }

    // ── ActualizadoHace ──────────────────────────────────────────────────────

    @Test fun `ActualizadoHace muestra minutos transcurridos`() {
        // Arrange: 12 minutos atrás
        val doceMin = 12 * 60 * 1_000L
        val ahoraMs = 1_000_000L
        val verificadoUtc = ahoraMs - doceMin
        // Act
        composeTestRule.setContent {
            ActualizadoHace(ahoraMs = ahoraMs, verificadoUtc = verificadoUtc)
        }
        // Assert
        composeTestRule
            .onNodeWithText("Actualizado hace 12 min")
            .assertIsDisplayed()
    }

    @Test fun `ActualizadoHace muestra horas cuando paso mas de una hora`() {
        // Arrange: 2 horas atrás
        val dosHoras = 2 * 3_600 * 1_000L
        val ahoraMs = 10_000_000L
        // Act
        composeTestRule.setContent {
            ActualizadoHace(ahoraMs = ahoraMs, verificadoUtc = ahoraMs - dosHoras)
        }
        // Assert
        composeTestRule
            .onNodeWithText("Actualizado hace 2 h")
            .assertIsDisplayed()
    }

    @Test fun `ActualizadoHace muestra Ahora cuando acaba de actualizar`() {
        // Arrange: 30 segundos atrás
        val ahoraMs = 2_000_000L
        // Act
        composeTestRule.setContent {
            ActualizadoHace(ahoraMs = ahoraMs, verificadoUtc = ahoraMs - 30_000L)
        }
        // Assert
        composeTestRule.onNodeWithText("Ahora").assertIsDisplayed()
    }

    @Test fun `ActualizadoHace muestra Sin actualizar cuando no hay verificadoUtc`() {
        // Arrange
        val ahoraMs = 1_000_000L
        // Act
        composeTestRule.setContent {
            ActualizadoHace(ahoraMs = ahoraMs, verificadoUtc = null)
        }
        // Assert
        composeTestRule.onNodeWithText("Sin actualizar").assertIsDisplayed()
    }
}
