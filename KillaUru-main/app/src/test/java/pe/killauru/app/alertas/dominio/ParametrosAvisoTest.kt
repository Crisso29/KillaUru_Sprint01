package pe.killauru.app.alertas.dominio

import org.junit.Test
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import kotlin.test.assertEquals

/** T097 / FR-056, RN-16: parámetros de aviso según categoría y velocidad. */
class ParametrosAvisoTest {
    private val params = ParametrosAviso()

    @Test
    fun `Liviano a 30 kmh usa distancia minima 100m`() {
        // Arrange / Act
        val dist = params.distanciaAviso(CategoriaVehiculo.LIVIANO, velocidadKmh = 30.0)
        // Assert
        assertEquals(100.0, dist, 0.1)
    }

    @Test
    fun `Liviano a 60 kmh da 133_3m`() {
        // Arrange / Act
        val dist = params.distanciaAviso(CategoriaVehiculo.LIVIANO, velocidadKmh = 60.0)
        // Assert
        assertEquals(133.3, dist, 0.5)
    }

    @Test
    fun `Pesado a 60 kmh da 200m`() {
        // Arrange / Act
        val dist = params.distanciaAviso(CategoriaVehiculo.PESADO, velocidadKmh = 60.0)
        // Assert
        assertEquals(200.0, dist, 0.5)
    }

    @Test
    fun `Moto usa mismos parametros que Liviano`() {
        // Arrange / Act
        val liviano = params.distanciaAviso(CategoriaVehiculo.LIVIANO, velocidadKmh = 60.0)
        val moto = params.distanciaAviso(CategoriaVehiculo.MOTO, velocidadKmh = 60.0)
        // Assert
        assertEquals(liviano, moto, 0.01)
    }

    @Test
    fun `Pesado a 30 kmh usa distancia minima 150m`() {
        // Arrange / Act
        val dist = params.distanciaAviso(CategoriaVehiculo.PESADO, velocidadKmh = 30.0)
        // Assert
        assertEquals(150.0, dist, 0.1)
    }
}
