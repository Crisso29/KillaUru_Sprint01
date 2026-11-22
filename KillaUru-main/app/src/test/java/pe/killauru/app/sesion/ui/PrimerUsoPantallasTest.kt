package pe.killauru.app.sesion.ui

import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.assertIsSelected
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performScrollTo
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import kotlin.test.assertEquals

/**
 * FR-001, FR-004, RN-18 y prototipo («Términos y privacidad», «¿Qué manejas?»): textos de cada pantalla y filas
 * pulsables enteras (el texto también marca; un conductor no tiene que acertar a un control de 20 dp). Patrón AAA.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class PrimerUsoPantallasTest {
    @get:Rule val compose = createComposeRule()

    private fun pulsar(texto: String) = compose.onNodeWithText(texto).performScrollTo().performClick()

    @Test fun `acepto y continuo se activa solo al marcar las dos casillas tocando su texto`() {
        // Arrange
        compose.setContent { TerminosPantalla("t", "p", version = "1.2", onAceptar = {}) }
        val boton = compose.onNodeWithText("Acepto y continúo")
        // Act + Assert
        boton.assertIsNotEnabled()
        pulsar("Acepto los términos de uso")
        boton.assertIsNotEnabled()
        pulsar("Acepto el aviso de privacidad")
        boton.assertIsEnabled()
    }

    @Test fun `los terminos dicen su version y que se volveran a pedir si cambia`() {
        // Arrange + Act
        compose.setContent { TerminosPantalla("t", "p", version = "1.2", onAceptar = {}) }
        // Assert
        compose.onNodeWithText("Versión 1.2. Si cambia, te la mostraremos otra vez.", substring = true).assertExists()
    }

    @Test fun `tocar la descripcion de una opcion elige ese vehiculo`() {
        // Arrange
        var elegido: CategoriaVehiculo? = null
        compose.setContent { VehiculoPantalla(onSeleccionar = { elegido = it }) }
        // Act
        pulsar("Auto, camioneta, station wagon o colectivo")
        compose.onNodeWithText("Continuar").performClick() // el botón vive en el pie fijo, fuera del desplazamiento
        // Assert
        compose.onNodeWithText("Liviano").assertIsSelected()
        assertEquals(CategoriaVehiculo.LIVIANO, elegido)
    }

    /** RN-18: cada opción dice qué pasará, para que nadie elija una categoría falsa sin querer. */
    @Test fun `cada vehiculo dice si detecta y la pantalla trae la guia del soporte y el cargador`() {
        // Arrange + Act
        compose.setContent { VehiculoPantalla(onSeleccionar = {}) }
        // Assert
        compose.onNodeWithText("Detecta baches, ve el mapa y recibe alertas").performScrollTo()
        compose.onNodeWithText("Mapa y alertas con más anticipación. No detecta.").performScrollTo()
        compose.onNodeWithText("Mapa y alertas. No detecta.").performScrollTo()
        compose.onNodeWithText("Celular en un soporte fijo.", substring = true).performScrollTo()
        compose.onNodeWithText("En turnos largos, conéctalo al cargador.", substring = true).performScrollTo()
    }

    @Test fun `continuar no hace nada si aun no se eligio vehiculo`() {
        // Arrange
        var llamado = false
        compose.setContent { VehiculoPantalla(onSeleccionar = { llamado = true }) }
        // Act
        compose.onNodeWithText("Continuar").assertIsNotEnabled()
        // Assert
        assertEquals(false, llamado)
    }
}
