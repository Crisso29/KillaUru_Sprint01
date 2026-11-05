package pe.killauru.app.mapa.dominio

import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T081 / FR-049, FR-051: punto dentro/fuera de la zona demo. */
class ZonaDemoTest {
    // Polígono simple: cuadrado 1°x1° alrededor de (-13.5, -74.5)
    private val zonaDemo = ZonaDemo(
        vertices = listOf(
            Coordenada(-13.0, -75.0),
            Coordenada(-13.0, -74.0),
            Coordenada(-14.0, -74.0),
            Coordenada(-14.0, -75.0),
        )
    )

    @Test
    fun `punto dentro del poligono devuelve true`() {
        // Arrange
        val punto = Coordenada(-13.5, -74.5)
        // Act / Assert
        assertTrue(zonaDemo.contiene(punto))
    }

    @Test
    fun `punto fuera del poligono devuelve false`() {
        // Arrange
        val punto = Coordenada(-10.0, -74.5)
        // Act / Assert
        assertFalse(zonaDemo.contiene(punto))
    }

    @Test
    fun `zona demo acepta hasta 60 vertices`() {
        // Arrange
        val vertices = (0 until 60).map { i ->
            val ang = 2 * Math.PI * i / 60
            Coordenada(-13.5 + 0.5 * Math.sin(ang), -74.5 + 0.5 * Math.cos(ang))
        }
        // Act
        val zona = ZonaDemo(vertices)
        // Assert
        assertTrue(zona.contiene(Coordenada(-13.5, -74.5)))
    }
}
