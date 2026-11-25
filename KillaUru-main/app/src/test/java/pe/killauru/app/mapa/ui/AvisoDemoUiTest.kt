package pe.killauru.app.mapa.ui

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

/**
 * T129 / FR-049, FR-051, RN-08: banner «Estás fuera del tramo demo».
 * Patrón AAA en cada test. Robolectric para Compose en JVM.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class AvisoDemoUiTest {

    @get:Rule val composeTestRule = createComposeRule()

    @Test fun `aviso visible cuando hay alerta`() {
        // Arrange + Act
        composeTestRule.setContent { AvisoDemoUi(visible = true, alDescartar = {}) }
        // Assert
        composeTestRule.onNodeWithText("Estás fuera del tramo demo").assertIsDisplayed()
    }

    @Test fun `aviso oculto cuando no hay alerta`() {
        // Arrange + Act
        composeTestRule.setContent { AvisoDemoUi(visible = false, alDescartar = {}) }
        // Assert
        composeTestRule.onNodeWithText("Estás fuera del tramo demo").assertDoesNotExist()
    }
}
