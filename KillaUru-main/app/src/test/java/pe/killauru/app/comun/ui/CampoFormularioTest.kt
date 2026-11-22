package pe.killauru.app.comun.ui

import androidx.compose.ui.test.assertIsFocused
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performImeAction
import androidx.compose.ui.test.performTextInput
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * Campo del prototipo (`.field` con `<label for>`), WCAG 1.3.1 y 4.1.2: el campo se llama como su etiqueta; la etiqueta no
 * se lee aparte (TalkBack no dice «Correo» dos veces) y tocarla enfoca el campo, como en la web. Patrón AAA; Robolectric.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class CampoFormularioTest {
    @get:Rule val compose = createComposeRule()

    @Test fun `el campo tiene el nombre de su etiqueta y la etiqueta no se repite para TalkBack`() {
        // Arrange
        var valor = ""
        // Act
        compose.setContent { CampoFormulario("Correo", valor, { valor = it }, TipoCampo.CORREO) }
        compose.onNodeWithContentDescription("Correo").performTextInput("a@b.pe")
        // Assert
        assertEquals("a@b.pe", valor)
        assertEquals(0, compose.onAllNodesWithText("Correo").fetchSemanticsNodes().size)
    }

    @Test fun `tocar la etiqueta enfoca el campo`() {
        // Arrange
        compose.setContent { CampoFormulario("Contraseña", "", {}, TipoCampo.CONTRASENA) }
        // Act
        compose.onNodeWithTag("etiqueta:Contraseña", useUnmergedTree = true).performClick()
        // Assert
        compose.onNodeWithContentDescription("Contraseña").assertIsFocused()
    }

    @Test fun `la tecla de confirmar del teclado envia el formulario`() {
        // Arrange
        var enviado = false
        compose.setContent { CampoFormulario("Correo", "a@b.pe", {}, TipoCampo.CORREO, alConfirmar = { enviado = true }) }
        // Act
        compose.onNodeWithContentDescription("Correo").performImeAction()
        // Assert
        assertTrue(enviado)
    }
}
