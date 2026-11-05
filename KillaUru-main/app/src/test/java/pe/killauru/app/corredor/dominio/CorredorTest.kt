package pe.killauru.app.corredor.dominio

import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

/** T080 / FR-055, RN-16: proyección del corredor en recta y curva. */
class CorredorTest {
    // Corredor simple: 3 vértices en línea recta sur→norte
    private val corredor = Corredor(
        vertices = listOf(
            Vertice(lat = -14.0, lon = -74.0, km = 0.0),
            Vertice(lat = -13.0, lon = -74.0, km = 111.0),
            Vertice(lat = -12.0, lon = -74.0, km = 222.0),
        )
    )

    @Test
    fun `punto cerca del segmento inicial proyecta correctamente`() {
        // Arrange: punto a ~10m del segmento, desplazado 0.0001° en longitud
        val punto = Coordenada(-13.5, -74.0001)
        // Act
        val pos = corredor.proyectar(punto)
        // Assert
        assertNotNull(pos)
        assertTrue(pos.km in 50.0..61.0, "km esperado ~55.5, fue ${pos.km}")
    }

    @Test
    fun `punto a mas de 50m de la via devuelve null`() {
        // Arrange: punto a ~5km de la vía
        val punto = Coordenada(-13.5, -78.0)
        // Act
        val pos = corredor.proyectar(punto)
        // Assert
        assertNull(pos)
    }

    @Test
    fun `busqueda incremental desde ultimo segmento`() {
        // Arrange: primera proyección cerca del km 55
        val p1 = Coordenada(-13.5, -74.0001)
        val pos1 = corredor.proyectar(p1)
        assertNotNull(pos1)
        // Act: proyección siguiente avanzando un poco
        val p2 = Coordenada(-13.49, -74.0001)
        val pos2 = corredor.proyectar(p2, desdeSegmento = pos1.segmento)
        // Assert
        assertNotNull(pos2)
        assertTrue(pos2.km >= pos1.km)
    }
}
