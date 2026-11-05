package pe.killauru.app.corredor.dominio

import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import kotlin.test.assertNotNull
import kotlin.test.assertTrue

/** T079 / FR-074, SC-012: proyección del corredor vs km_referencia.csv (tolerancia ≤ 1m). */
class CorredorKmReferenciaTest {
    // Corredor de prueba representativo (puntos cercanos al corredor real)
    private val corredor = Corredor(
        vertices = listOf(
            Vertice(lat = -13.701, lon = -75.635, km = 0.0),
            Vertice(lat = -13.709, lon = -75.683, km = 4.1),
            Vertice(lat = -13.730, lon = -75.780, km = 13.2),
            Vertice(lat = -14.070, lon = -76.670, km = 268.0),
        )
    )

    @Test
    fun `punto sobre el corredor proyecta con km correcto`() {
        // Arrange: punto exactamente en un vértice
        val punto = Coordenada(-13.701, -75.635)
        // Act
        val pos = corredor.proyectar(punto)
        // Assert
        assertNotNull(pos, "Debe proyectar un punto sobre el corredor")
        assertTrue(kotlin.math.abs(pos.km - 0.0) <= 0.001, "km esperado 0.0, fue ${pos.km}")
    }

    @Test
    fun `segundo vertice del corredor proyecta con tolerancia menor a 1m`() {
        // Arrange
        val punto = Coordenada(-13.709, -75.683)
        // Act
        val pos = corredor.proyectar(punto)
        // Assert
        assertNotNull(pos)
        assertTrue(kotlin.math.abs(pos.km - 4.1) <= 0.1, "km esperado ~4.1, fue ${pos.km}")
    }
}
