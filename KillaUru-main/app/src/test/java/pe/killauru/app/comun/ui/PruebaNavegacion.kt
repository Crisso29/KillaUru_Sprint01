package pe.killauru.app.comun.ui

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import org.junit.Rule

/** Base de las pruebas del grafo: abre la app con [PantallasDobles] y deja pulsar y comprobar por texto. */
abstract class PruebaNavegacion {
    @get:Rule val compose = createComposeRule()
    protected lateinit var nav: NavHostController

    protected fun abrirApp() = compose.setContent {
        nav = rememberNavController()
        NavGrafo(pantallas = PantallasDobles, navController = nav)
    }

    protected fun pulsar(texto: String) = compose.onNodeWithText(texto).performClick()

    protected fun seVe(texto: String) = compose.onNodeWithText(texto).assertIsDisplayed()
}
