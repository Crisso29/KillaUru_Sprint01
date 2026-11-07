package pe.killauru.app.sesion.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T025 / FR-001, FR-003: sin las dos casillas no se puede aceptar; versión distinta → pedir de nuevo. */
class ConsentimientoTest {
    @Test
    fun `requiere terminos y politica para aceptar`() {
        // Arrange
        val c = Consentimiento(terminosVersion = "1.2", politicaVersion = "1.2")
        // Act / Assert
        assertFalse(c.puedeAceptar(terminosMarcado = false, politicaMarcada = false))
        assertFalse(c.puedeAceptar(terminosMarcado = true, politicaMarcada = false))
        assertFalse(c.puedeAceptar(terminosMarcado = false, politicaMarcada = true))
        assertTrue(c.puedeAceptar(terminosMarcado = true, politicaMarcada = true))
    }

    @Test
    fun `version distinta a la vigente requiere pedir de nuevo`() {
        // Arrange
        val vigente = Consentimiento(terminosVersion = "1.2", politicaVersion = "1.2")
        val aceptada = VersionesAceptadas(terminosVersion = "1.1", politicaVersion = "1.2")
        // Act
        val necesita = vigente.necesitaRenovacion(aceptada)
        // Assert
        assertTrue(necesita)
    }

    @Test
    fun `versiones iguales no necesitan renovacion`() {
        // Arrange
        val vigente = Consentimiento(terminosVersion = "1.2", politicaVersion = "1.2")
        val aceptada = VersionesAceptadas(terminosVersion = "1.2", politicaVersion = "1.2")
        // Act
        val necesita = vigente.necesitaRenovacion(aceptada)
        // Assert
        assertFalse(necesita)
    }

    @Test
    fun `politica de version distinta tambien requiere renovacion`() {
        // Arrange
        val vigente = Consentimiento(terminosVersion = "1.2", politicaVersion = "1.2")
        val aceptada = VersionesAceptadas(terminosVersion = "1.2", politicaVersion = "1.1")
        // Act
        val necesita = vigente.necesitaRenovacion(aceptada)
        // Assert
        assertTrue(necesita)
    }
}
