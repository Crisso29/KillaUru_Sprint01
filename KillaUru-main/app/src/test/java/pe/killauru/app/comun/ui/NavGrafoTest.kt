package pe.killauru.app.comun.ui

import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import kotlin.test.assertNull

/**
 * T024, T126, FR-008, RNF-12: a dónde lleva cada acción de la app y en cuántos toques (pestañas en 1, detalle en 2).
 * Las pantallas son dobles (un texto y un botón), así la prueba mira solo la navegación, sin Hilt, GPS ni mapa.
 * Patrón AAA; Robolectric para Compose en la JVM.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class NavGrafoTest {
    @get:Rule val compose = createComposeRule()
    private lateinit var nav: NavHostController

    private val dobles = object : PantallasApp {
        @Composable override fun PrimerUso(alCompletar: () -> Unit) = Boton("terminar primer uso", alCompletar)
        @Composable override fun Mapa(alVerDetalle: (String) -> Unit) = Boton("pantalla mapa") { alVerDetalle("a-1") }
        @Composable override fun Historial() = Text("pantalla historial")
        @Composable override fun Ajustes() = Text("pantalla ajustes")
        @Composable override fun Detalle(anomaliaId: String, alVolver: () -> Unit) = Boton("detalle $anomaliaId", alVolver)
    }

    @Composable private fun Boton(texto: String, alPulsar: () -> Unit) = Button(onClick = alPulsar) { Text(texto) }

    private fun abrirApp() = compose.setContent {
        nav = rememberNavController()
        NavGrafo(pantallas = dobles, navController = nav)
    }

    private fun pulsar(texto: String) = compose.onNodeWithText(texto).performClick()

    private fun seVe(texto: String) = compose.onNodeWithText(texto).assertIsDisplayed()

    @Test fun `la app empieza en el primer uso y sin barra inferior`() {
        // Arrange + Act
        abrirApp()
        // Assert
        seVe("terminar primer uso")
        compose.onNodeWithText("Mis viajes").assertDoesNotExist()
    }

    /** Volver atrás desde el mapa cierra la app: no regresa a los términos ya aceptados. */
    @Test fun `al completar el primer uso va al mapa y lo saca de la pila`() {
        // Arrange
        abrirApp()
        // Act
        pulsar("terminar primer uso")
        // Assert
        seVe("pantalla mapa")
        compose.runOnIdle { assertNull(nav.previousBackStackEntry) }
    }

    @Test fun `la barra inferior cambia entre mapa, mis viajes y ajustes`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        // Act + Assert (cada pestaña es un paso del recorrido)
        pulsar("Mis viajes"); seVe("pantalla historial")
        pulsar("Ajustes"); seVe("pantalla ajustes")
        pulsar("Mapa"); seVe("pantalla mapa")
    }

    @Test fun `el detalle recibe el id de la anomalia y vuelve al mapa`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        // Act
        pulsar("pantalla mapa")
        seVe("detalle a-1")
        pulsar("detalle a-1")
        // Assert
        seVe("pantalla mapa")
    }
}
