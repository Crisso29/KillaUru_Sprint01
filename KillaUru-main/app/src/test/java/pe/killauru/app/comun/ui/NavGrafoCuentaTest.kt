package pe.killauru.app.comun.ui

import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import kotlin.test.assertEquals
import kotlin.test.assertNull

/**
 * T024, FR-008, FR-060…FR-069: rutas de cuenta. Tras entrar, verificar, cerrar sesión o eliminar se vuelve al mapa sin
 * formularios en la pila; el perfil queda a 2 toques del mapa. Patrón AAA; Robolectric.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class NavGrafoCuentaTest : PruebaNavegacion() {
    @Test fun `crear cuenta en el primer uso abre el registro sobre el mapa`() {
        // Arrange
        abrirApp()
        // Act
        pulsar("primer uso crear cuenta")
        // Assert — atrás desde el registro vuelve al mapa, no a la bienvenida
        seVe("registro crear")
        compose.runOnIdle { assertEquals(Rutas.Mapa.ruta, nav.previousBackStackEntry?.destination?.route) }
    }

    @Test fun `ya tengo cuenta en el primer uso abre el inicio de sesion`() {
        // Arrange
        abrirApp()
        // Act
        pulsar("primer uso ya tengo cuenta")
        // Assert
        seVe("login entrar")
    }

    @Test fun `al crear la cuenta pide revisar el correo con el correo intacto y luego va al mapa`() {
        // Arrange
        abrirApp()
        pulsar("primer uso crear cuenta")
        // Act — el «+» del correo no debe perderse al pasar por la ruta
        pulsar("registro crear")
        seVe("verificar conductor+1@ejemplo.pe")
        pulsar("verificar conductor+1@ejemplo.pe")
        // Assert — ni el registro ni la verificación quedan en la pila
        seVe("pantalla mapa")
        compose.runOnIdle { assertNull(nav.previousBackStackEntry) }
    }

    @Test fun `iniciar sesion desde ajustes y olvide mi contrasena`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        pulsar("Ajustes")
        // Act + Assert
        pulsar("ajustes iniciar sesion"); seVe("login olvide")
        pulsar("login olvide"); seVe("olvide volver al login")
        pulsar("olvide volver al login"); seVe("login entrar")
        pulsar("login entrar"); seVe("pantalla mapa")
    }

    /** FR-008: el perfil está a 2 toques del mapa (Ajustes → Mi perfil). */
    @Test fun `del perfil se cierra sesion y se vuelve al mapa`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        pulsar("Ajustes")
        pulsar("ajustes perfil")
        // Act
        pulsar("perfil cerrar sesion")
        pulsar("confirmar cierre")
        // Assert
        seVe("pantalla mapa")
        compose.runOnIdle { assertNull(nav.previousBackStackEntry) }
    }

    @Test fun `cancelar el cierre de sesion vuelve al perfil`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        pulsar("Ajustes")
        pulsar("ajustes perfil")
        pulsar("perfil cerrar sesion")
        // Act
        pulsar("cancelar cierre")
        // Assert
        seVe("perfil eliminar")
    }

    @Test fun `al eliminar la cuenta vuelve al mapa`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        pulsar("Ajustes")
        pulsar("ajustes perfil")
        // Act
        pulsar("perfil eliminar")
        pulsar("confirmar eliminar")
        // Assert
        seVe("pantalla mapa")
    }

    @Test fun `mis viajes sin cuenta lleva a crear cuenta`() {
        // Arrange
        abrirApp()
        pulsar("terminar primer uso")
        pulsar("Mis viajes")
        // Act
        pulsar("pantalla historial")
        // Assert
        seVe("registro crear")
    }
}
