package pe.killauru.app.alertas.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T099 / FR-054, FR-059, FR-063: solo registrados con permiso vigente reciben alertas. */
class ElegibilidadAlertasTest {
    private val elegibilidad = ElegibilidadAlertas()

    @Test
    fun `anonimo no recibe alertas`() {
        // Arrange
        val estado = EstadoElegibilidad(registrado = false, permisoVigente = true,
            soloVerMapa = false, enAhorro = false)
        // Act / Assert
        assertFalse(elegibilidad.esElegible(estado))
    }

    @Test
    fun `registrado con permiso vigente es elegible`() {
        // Arrange
        val estado = EstadoElegibilidad(registrado = true, permisoVigente = true,
            soloVerMapa = false, enAhorro = false)
        // Act / Assert
        assertTrue(elegibilidad.esElegible(estado))
    }

    @Test
    fun `registrado con solo ver mapa no recibe alertas`() {
        // Arrange
        val estado = EstadoElegibilidad(registrado = true, permisoVigente = true,
            soloVerMapa = true, enAhorro = false)
        // Act / Assert
        assertFalse(elegibilidad.esElegible(estado))
    }

    @Test
    fun `registrado en ahorro no recibe alertas`() {
        // Arrange
        val estado = EstadoElegibilidad(registrado = true, permisoVigente = true,
            soloVerMapa = false, enAhorro = true)
        // Act / Assert
        assertFalse(elegibilidad.esElegible(estado))
    }

    @Test
    fun `registrado con permiso vencido no es elegible`() {
        // Arrange
        val estado = EstadoElegibilidad(registrado = true, permisoVigente = false,
            soloVerMapa = false, enAhorro = false)
        // Act / Assert
        assertFalse(elegibilidad.esElegible(estado))
    }
}
