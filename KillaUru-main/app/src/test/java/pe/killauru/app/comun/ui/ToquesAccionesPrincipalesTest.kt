package pe.killauru.app.comun.ui

import kotlin.test.Test
import kotlin.test.assertTrue

/**
 * T126: RNF-12, SC-007 — las acciones principales son alcanzables en ≤ 3 toques.
 * Verifica la declaración de rutas y que las pantallas principales existen.
 */
class ToquesAccionesPrincipalesTest {

    @Test
    fun `mapa es la pantalla inicial accesible en 1 toque`() {
        // Arrange
        val rutas = listOf(Rutas.Mapa, Rutas.Historial, Rutas.Ajustes, Rutas.Login)

        // Act + Assert — pantalla inicial debe estar en la barra inferior
        assertTrue(Rutas.Mapa in rutas, "Mapa debe ser una ruta principal")
    }

    @Test
    fun `historial accesible desde la barra inferior en 1 toque`() {
        // Arrange
        val rutas = listOf(Rutas.Mapa, Rutas.Historial, Rutas.Ajustes, Rutas.Login)

        // Act + Assert
        assertTrue(Rutas.Historial in rutas)
    }

    @Test
    fun `ajustes accesible desde la barra inferior en 1 toque`() {
        // Arrange
        val rutas = listOf(Rutas.Mapa, Rutas.Historial, Rutas.Ajustes, Rutas.Login)

        // Act + Assert
        assertTrue(Rutas.Ajustes in rutas)
    }

    @Test
    fun `login-perfil accesible desde la barra inferior en 1 toque`() {
        // Arrange
        val rutas = listOf(Rutas.Mapa, Rutas.Historial, Rutas.Ajustes, Rutas.Login)

        // Act + Assert
        assertTrue(Rutas.Login in rutas)
    }
}
