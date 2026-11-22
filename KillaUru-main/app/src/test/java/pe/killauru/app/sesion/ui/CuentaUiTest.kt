package pe.killauru.app.sesion.ui

import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performScrollTo
import androidx.compose.ui.test.performTextInput
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import pe.killauru.app.sesion.dominio.Aceptacion
import pe.killauru.app.sesion.dominio.DocumentoLegal
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.Rol
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertNull

/**
 * T110 / FR-060, FR-061, FR-064…FR-069 y prototipo (pantallas 16–23): lo que ve y puede tocar el conductor en cada
 * pantalla de cuenta. Los ViewModels tienen su propia prueba; aquí se pasa el estado. Patrón AAA; Robolectric.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [33])
class CuentaUiTest {
    @get:Rule val compose = createComposeRule()

    private fun escribir(campo: String, texto: String) = compose.onNodeWithContentDescription(campo).performTextInput(texto)

    private fun pulsar(texto: String) = compose.onNodeWithText(texto).performScrollTo().performClick()

    /** El botón del pie que repite el título de la pantalla («Crear cuenta», «Eliminar mi cuenta»): [0] es el título. */
    private fun boton(texto: String) = compose.onAllNodesWithText(texto)[1]

    private fun seVe(texto: String) = compose.onNodeWithText(texto, substring = true).assertExists()

    @Test fun `crear cuenta exige marcar el consentimiento y envia correo sin espacios`() {
        // Arrange
        var enviado: Pair<String, String>? = null
        compose.setContent { RegistroPantalla(RegistroUiState(politicaVersion = "1.2"), alCrear = { c, p -> enviado = c to p }) }
        escribir("Correo", " conductor@ejemplo.pe ")
        escribir("Contraseña", "una frase larga")
        // Act + Assert — sin consentimiento no se puede (FR-003)
        boton("Crear cuenta").assertIsNotEnabled()
        pulsar("Acepto la política de privacidad (v1.2) y los términos de uso")
        boton("Crear cuenta").assertIsEnabled().performClick()
        assertEquals("conductor@ejemplo.pe" to "una frase larga", enviado)
    }

    @Test fun `crear cuenta explica la contrasena y que se guarda`() {
        // Arrange + Act
        compose.setContent { RegistroPantalla(RegistroUiState(), alCrear = { _, _ -> }) }
        // Assert
        seVe("Mínimo 10 caracteres")
        seVe("Guardamos tu correo, tu contraseña cifrada y la fecha en que aceptaste")
    }

    @Test fun `revisa tu correo no bloquea y deja reenviar una vez`() {
        // Arrange
        var reenvios = 0
        compose.setContent { VerificarCorreoPantalla("conductor@ejemplo.pe", reenviado = false, alIrAlMapa = {}, alReenviar = { reenvios++ }) }
        // Act
        pulsar("Reenviar el correo")
        // Assert
        seVe("conductor@ejemplo.pe")
        seVe("Ya puedes usar los 333 km")
        compose.onNodeWithText("Ir al mapa").assertIsEnabled()
        assertEquals(1, reenvios)
    }

    @Test fun `login muestra el error generico y el enlace a olvide`() {
        // Arrange
        var olvide = false
        val error = "Correo o contraseña incorrectos. Si fallas varias veces, espera unos minutos."
        compose.setContent { LoginPantalla(LoginUiState(error = error), alEntrar = { _, _ -> }, alOlvide = { olvide = true }) }
        // Act
        pulsar("Olvidé mi contraseña")
        // Assert
        seVe(error)
        compose.onNodeWithText("Entrar").assertIsNotEnabled() // sin correo ni contraseña
        assertEquals(true, olvide)
    }

    @Test fun `olvide confirma el envio y ofrece volver a iniciar sesion`() {
        // Arrange
        var volvio = false
        compose.setContent { OlvideContrasenaPantalla(OlvideUiState(enviado = true), alEnviar = {}, alVolverAlLogin = { volvio = true }) }
        // Act
        compose.onNodeWithText("Volver a iniciar sesión").performClick() // está en el pie fijo, no se desplaza
        // Assert
        seVe("Si el correo está registrado, te llegará un enlace que vence en 30 minutos")
        assertEquals(true, volvio)
    }

    @Test fun `el perfil muestra todo lo que se guarda`() {
        // Arrange
        val alta = Instant.parse("2026-10-14T15:00:00Z").toEpochMilli()
        val datos = MisDatos("conductor@ejemplo.pe", Rol.ConductorRegistrado, alta, true,
            listOf(Aceptacion(DocumentoLegal.PoliticaPrivacidad, "1.2", alta), Aceptacion(DocumentoLegal.TerminosUso, "1.2", alta)))
        // Act
        compose.setContent { PerfilPantalla(PerfilUiState(cargando = false, datos = datos), {}, {}, {}) }
        // Assert
        listOf("conductor@ejemplo.pe", "Conductor registrado", "14 oct 2026", "Sí").forEach { compose.onNodeWithText(it).assertExists() }
        assertEquals(2, compose.onAllNodesWithText("v1.2 · 14 oct 2026").fetchSemanticsNodes().size)
        seVe("no están unidos a tu cuenta")
    }

    @Test fun `el perfil sin red lo dice y deja reintentar`() {
        // Arrange
        var reintentos = 0
        compose.setContent { PerfilPantalla(PerfilUiState(cargando = false, error = "Sin red."), { reintentos++ }, {}, {}) }
        // Act
        pulsar("Reintentar")
        // Assert
        seVe("Sin red.")
        assertEquals(1, reintentos)
    }

    @Test fun `cerrar sesion avisa de las 3 detecciones por enviar`() {
        // Arrange + Act
        compose.setContent { CerrarSesionPantalla(pendientes = 3, alConfirmar = {}, alCancelar = {}) }
        // Assert
        seVe("3 detecciones por enviar no se pierden")
        seVe("Tu historial de viajes se borra")
    }

    @Test fun `sin pendientes no habla de detecciones`() {
        // Arrange + Act
        val lineas = lineasCierreSesion(0)
        // Assert
        assertNull(lineas.firstOrNull { "detecci" in it })
        assertEquals(true, lineasCierreSesion(1).any { "**detección por enviar no se pierde**" in it })
    }

    @Test fun `eliminar exige contrasena y marcar entiendo`() {
        // Arrange
        var contrasena: String? = null
        compose.setContent { EliminarCuentaPantalla(EliminarUiState(), alEliminar = { contrasena = it }, alCancelar = {}) }
        // Act + Assert
        escribir("Contraseña", "mi-clave-larga")
        boton("Eliminar mi cuenta").assertIsNotEnabled()
        pulsar("Entiendo que no se puede deshacer")
        boton("Eliminar mi cuenta").assertIsEnabled().performClick()
        assertEquals("mi-clave-larga", contrasena)
    }

    @Test fun `eliminar con contrasena equivocada muestra el error`() {
        // Arrange + Act
        compose.setContent { EliminarCuentaPantalla(EliminarUiState(error = "La contraseña no es correcta."), {}, {}) }
        // Assert
        seVe("La contraseña no es correcta.")
    }
}
