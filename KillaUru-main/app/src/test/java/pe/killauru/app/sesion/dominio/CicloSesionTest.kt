package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.casos.AceptarVersionesUseCase
import pe.killauru.app.sesion.dominio.casos.AplicarSesionInvalidaUseCase
import pe.killauru.app.sesion.dominio.casos.LimpiezaLocalSesion
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.RecorridosMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** FR-003, FR-063, RN-21 c: consentimiento y limpieza de la identidad local. Iniciar, registrar y eliminar tienen su archivo. */
class CicloSesionTest {

    /** RN-21 c: ante `cuenta_desactivada` o `sesion_invalida` se borran tokens, permiso y preferencias de identidad. */
    @Test fun `sesion invalida limpia la identidad local`() = runTest {
        // Arrange
        val repo = SesionMemoria(SesionesDePrueba.de(Rol.ConductorRegistrado))
        val prefs = PreferenciasMemoria()
        val caso = AplicarSesionInvalidaUseCase(repo, LimpiezaLocalSesion(repo, prefs, RecorridosMemoria()))
        // Act
        caso.ejecutar()
        // Assert
        assertNull(repo.actual)
        assertEquals(1, prefs.limpiezas)
    }

    /** FR-003: ambas versiones se guardan con la misma hora de aceptación. */
    @Test fun `consentimiento persiste ambas versiones con la misma hora`() = runTest {
        // Arrange
        val prefs = PreferenciasMemoria()
        val caso = AceptarVersionesUseCase(prefs) { 1234 }
        // Act
        caso.aceptar("2", "3")
        // Assert
        assertEquals("2", prefs.terminosVersion)
        assertEquals("3", prefs.politicaVersion)
        assertEquals(1234L, prefs.terminosAceptadoUtc)
        assertEquals(prefs.terminosAceptadoUtc, prefs.politicaAceptadoUtc)
    }
}
