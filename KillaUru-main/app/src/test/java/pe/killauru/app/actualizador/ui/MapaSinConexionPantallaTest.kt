package pe.killauru.app.actualizador.ui

import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performScrollTo
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import pe.killauru.app.mapa.dominio.EstadoDescarga
import kotlin.test.assertEquals

/** FR-041, FR-043 (prototipo 05, «Mapa sin conexión»): textos y acciones del paso 4. Patrón AAA; Robolectric. */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class MapaSinConexionPantallaTest {
    @get:Rule val compose = createComposeRule()

    @Test fun `el peso se muestra en MB con coma decimal`() {
        // Arrange + Act + Assert
        assertEquals("2,3 MB", TextosDescarga.peso(2_362_855)) // MB binarios, como el «38 MB» del contrato
        assertEquals("38 MB", TextosDescarga.peso(39_845_120))
        assertEquals("", TextosDescarga.peso(0))
    }

    @Test fun `cada estado explica que pasa con palabras`() {
        // Arrange + Act + Assert — el estado no se comunica solo con la barra (WCAG 1.4.1)
        assertEquals("Si se corta, sigue desde donde quedó.", TextosDescarga.nota(EstadoDescarga.Descargando(62)))
        assertEquals("Esperando Wi-Fi. Activa los datos móviles si quieres descargarlo ya.",
            TextosDescarga.nota(EstadoDescarga.EsperandoWifi(2_362_855)))
        assertEquals("Listo: el mapa ya está en tu celular.", TextosDescarga.nota(EstadoDescarga.Listo))
    }

    @Test fun `durante la descarga muestra el corredor, el peso y el porcentaje y deja seguir`() {
        // Arrange
        var siguio = false
        compose.setContent {
            MapaSinConexionPantalla(PantallaDescargaUi(EstadoDescarga.Descargando(62, 2_362_855)), {}, alSeguir = { siguio = true })
        }
        // Act
        compose.onNodeWithText("Seguir mientras descarga").performClick()
        // Assert
        compose.onNodeWithText("Corredor PE-28A · 335 km · 2,3 MB").assertExists()
        compose.onNodeWithText("62 %").assertExists()
        compose.onNodeWithText("Si se corta, sigue desde donde quedó.").assertExists()
        assertEquals(true, siguio)
    }

    @Test fun `el interruptor de datos moviles dice su efecto y avisa el cambio`() {
        // Arrange
        var pedido: Boolean? = null
        compose.setContent {
            MapaSinConexionPantalla(PantallaDescargaUi(EstadoDescarga.Descargando(0)), alCambiarMoviles = { pedido = it }, alSeguir = {})
        }
        // Act
        compose.onNodeWithText("Usar datos móviles").performScrollTo().performClick()
        // Assert
        compose.onNodeWithText("Apagado: solo con Wi-Fi").assertExists()
        assertEquals(true, pedido)
    }

    @Test fun `con el mapa listo el boton dice continuar`() {
        // Arrange + Act
        compose.setContent { MapaSinConexionPantalla(PantallaDescargaUi(EstadoDescarga.Listo), {}, {}) }
        // Assert
        compose.onNodeWithText("Continuar").assertExists()
        compose.onNodeWithText("100 %").assertExists()
    }
}
