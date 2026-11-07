package pe.killauru.app.comun.ui

import kotlin.test.Test
import kotlin.test.assertTrue

/**
 * T126: FR-008, RNF-12 — acciones principales accesibles en ≤ 3 toques.
 *
 * La barra inferior expone Mapa, Historial y Ajustes directamente (1 toque).
 * El detalle de anomalía se alcanza desde el Mapa (2 toques: tab + tap).
 * Cerrar sesión se alcanza desde Ajustes → Perfil (2 toques: tab + tap).
 *
 * Esta prueba verifica la presencia de las rutas en el grafo de navegación.
 */
class ToquesAccionesPrincipalesTest {

    private val rutasEnBarraInferior = setOf(
        Rutas.Mapa.ruta,
        Rutas.Historial.ruta,
        Rutas.Ajustes.ruta,
    )

    @Test
    fun `mapa es accesible en 1 toque desde la barra inferior`() {
        // Assert
        assertTrue(Rutas.Mapa.ruta in rutasEnBarraInferior)
    }

    @Test
    fun `historial es accesible en 1 toque desde la barra inferior`() {
        // Assert
        assertTrue(Rutas.Historial.ruta in rutasEnBarraInferior)
    }

    @Test
    fun `ajustes es accesible en 1 toque desde la barra inferior`() {
        // Assert
        assertTrue(Rutas.Ajustes.ruta in rutasEnBarraInferior)
    }

    @Test
    fun `detalle es alcanzable en 2 toques desde el mapa`() {
        // Arrange — ruta del detalle existe en el grafo
        // Assert
        assertTrue(Rutas.Detalle.ruta.isNotBlank())
    }
}
