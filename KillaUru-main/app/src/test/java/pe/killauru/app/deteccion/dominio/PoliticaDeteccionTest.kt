package pe.killauru.app.deteccion.dominio

import org.junit.Test
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T037 / FR-010, FR-005, RN-18: tabla de verdad de la política de detección. */
class PoliticaDeteccionTest {
    private val politica = PoliticaDeteccion(listOf("LIVIANO"))

    private fun estado(
        categoria: CategoriaVehiculo? = CategoriaVehiculo.LIVIANO,
        soloVerMapa: Boolean = false,
        enAhorro: Boolean = false,
        tienePermiso: Boolean = true,
    ) = PoliticaDeteccion.EstadoDeteccion(categoria, soloVerMapa, enAhorro, tienePermiso)

    @Test
    fun `liviano con todo en verde puede detectar`() {
        // Arrange / Act / Assert
        assertTrue(politica.puedeDetectar(estado()))
    }

    @Test
    fun `solo ver mapa impide detectar`() {
        // Arrange / Act / Assert
        assertFalse(politica.puedeDetectar(estado(soloVerMapa = true)))
    }

    @Test
    fun `ahorro de bateria impide detectar`() {
        // Arrange / Act / Assert
        assertFalse(politica.puedeDetectar(estado(enAhorro = true)))
    }

    @Test
    fun `sin permiso de ubicacion impide detectar`() {
        // Arrange / Act / Assert
        assertFalse(politica.puedeDetectar(estado(tienePermiso = false)))
    }

    @Test
    fun `pesado no detecta cuando solo liviano esta en la lista`() {
        // Arrange / Act / Assert
        assertFalse(politica.puedeDetectar(estado(categoria = CategoriaVehiculo.PESADO)))
    }

    @Test
    fun `sin categoria no puede detectar`() {
        // Arrange / Act / Assert
        assertFalse(politica.puedeDetectar(estado(categoria = null)))
    }
}
