package pe.killauru.app.comun.ui

import androidx.compose.ui.test.onNodeWithText
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import kotlin.test.assertNull

/**
 * T024, T126, FR-008, RNF-12: a dónde lleva cada acción principal y en cuántos toques (pestañas en 1, detalle en 2).
 * Patrón AAA; Robolectric para Compose en la JVM. Las rutas de cuenta están en [NavGrafoCuentaTest].
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class NavGrafoTest : PruebaNavegacion() {
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
