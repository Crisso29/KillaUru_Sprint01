package pe.killauru.app.comun.ui

import androidx.compose.material3.Text
import androidx.compose.ui.test.assertIsSelected
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import pe.killauru.app.comun.ui.tema.KillaUruTema
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** Componentes del prototipo (`.appbar`, `.steps`, `.opt`, `.set`) y sus íconos. Patrón AAA; Robolectric para Compose. */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class ComponentesPrototipoTest {
    @get:Rule val compose = createComposeRule()

    /** Un trazado mal convertido por el generador fallaría aquí y no en el teléfono del conductor. */
    @Test fun `cada icono generado del prototipo se puede dibujar`() {
        // Arrange
        val nombres = IconosKillaUru.nombres
        // Act
        val vectores = nombres.map { IconosKillaUru.de(it) }
        // Assert
        assertTrue(nombres.size >= 30)
        assertEquals(nombres.size, vectores.count { it.root.size > 0 })
    }

    @Test fun `la pantalla muestra el paso y el boton volver avisa`() {
        // Arrange
        var volvio = false
        compose.setContent {
            KillaUruTema { PantallaApp("¿Qué manejas?", alVolver = { volvio = true }, paso = Paso(2, 5)) { Text("cuerpo") } }
        }
        // Act
        compose.onNodeWithContentDescription("Volver").performClick()
        // Assert
        compose.onNodeWithText("Paso 2 de 5").assertExists()
        assertTrue(volvio)
    }

    @Test fun `tocar cualquier parte de la tarjeta la elige`() {
        // Arrange
        var elegida = false
        compose.setContent {
            KillaUruTema {
                TarjetaOpcion("car", "Liviano", "Auto o camioneta", elegida = elegida, alElegir = { elegida = true },
                    efecto = "Detecta baches")
            }
        }
        // Act
        compose.onNodeWithText("Detecta baches").performClick()
        // Assert
        assertTrue(elegida)
    }

    @Test fun `la tarjeta elegida se anuncia como seleccionada`() {
        // Arrange + Act
        compose.setContent { KillaUruTema { TarjetaOpcion("moto", "Moto", "Moto o mototaxi", elegida = true, alElegir = {}) } }
        // Assert
        compose.onNodeWithText("Moto").assertIsSelected()
    }

    @Test fun `una fila de ajuste con accion es pulsable entera`() {
        // Arrange
        var abierta = false
        compose.setContent { KillaUruTema { FilaAjuste("Mi perfil", detalle = "conductor@ejemplo.pe", alPulsar = { abierta = true }) } }
        // Act
        compose.onNodeWithText("conductor@ejemplo.pe").performClick()
        // Assert
        assertTrue(abierta)
    }
}
