package pe.killauru.app.deteccion.dominio

import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse

/** T037 / RN-22: el ahorro no cancela las alertas, solo detección y envío. */
class PoliticaBateriaTest {
    @Test
    fun `solo entra en ahorro por debajo del umbral sin cargar`() {
        // Arrange
        val politica = PoliticaBateria(15)
        // Act
        val resultados = listOf(14, 15, 16).map { politica.enAhorro(it, false) }
        // Assert
        assertEquals(listOf(true, false, false), resultados)
    }

    @Test
    fun `cargar permite reanudar con bateria baja`() {
        // Arrange
        val politica = PoliticaBateria(15)
        // Act
        val ahorro = politica.enAhorro(14, true)
        // Assert
        assertFalse(ahorro)
    }
}
