package pe.killauru.app.comun.dominio

import org.junit.Test
import kotlin.test.assertEquals

/** T010 / FR-055, FR-074: mismo radio y aproximación que el corredor del servidor. */
class GeodesiaTest {
    @Test
    fun `un grado de meridiano mide ciento once kilometros`() {
        // Arrange
        val inicio = Coordenada(0.0, 0.0)
        val fin = Coordenada(1.0, 0.0)
        // Act
        val metros = Geodesia.distanciaMetros(inicio, fin)
        // Assert
        assertEquals(111_195.0802, metros, 0.001)
        assertEquals(0.0, Geodesia.distanciaMetros(inicio, inicio), 0.001)
    }

    @Test
    fun `proyeccion limita el punto al segmento`() {
        // Arrange
        val inicio = Coordenada(0.0, 0.0)
        val fin = Coordenada(0.0, 1.0)
        // Act
        val proporciones = listOf(-0.5, 0.5, 1.5).map {
            Geodesia.proyectar(Coordenada(0.1, it), inicio, fin).fraccion
        }
        // Assert
        assertEquals(listOf(0.0, 0.5, 1.0), proporciones)
        assertEquals(0.0, Geodesia.proyectar(fin, inicio, inicio).fraccion)
    }
}
