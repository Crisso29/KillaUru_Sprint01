package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.casos.EliminarCuentaUseCase
import pe.killauru.app.sesion.dominio.casos.LimpiezaLocalSesion
import pe.killauru.app.soporte.ApiCuentaDoble
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.RecorridosMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** T107 / FR-067, HU-23: eliminar pide la contraseña; solo un 204 limpia la app, un 403 no borra nada. */
class EliminarCuentaUseCaseTest {
    private val sesion = SesionesDePrueba.de(Rol.ConductorRegistrado)

    @Test fun `con 204 envia la contrasena y hace la limpieza local`() = runTest {
        // Arrange
        val api = ApiCuentaDoble(resultadoEliminacion = ResultadoEliminacion.Eliminada)
        val repo = SesionMemoria(sesion)
        val prefs = PreferenciasMemoria()
        val caso = EliminarCuentaUseCase(api, repo, LimpiezaLocalSesion(repo, prefs, RecorridosMemoria()))
        // Act
        val resultado = caso.ejecutar("una-clave-larga")
        // Assert
        assertEquals(ResultadoEliminacion.Eliminada, resultado)
        assertEquals("una-clave-larga", api.ultimaContrasenaEliminacion)
        assertNull(repo.actual)
        assertEquals(1, prefs.limpiezas)
    }

    @Test fun `con 403 confirmacion_invalida no borra nada`() = runTest {
        // Arrange
        val api = ApiCuentaDoble(resultadoEliminacion = ResultadoEliminacion.ConfirmacionInvalida)
        val repo = SesionMemoria(sesion)
        val prefs = PreferenciasMemoria()
        val caso = EliminarCuentaUseCase(api, repo, LimpiezaLocalSesion(repo, prefs, RecorridosMemoria()))
        // Act
        val resultado = caso.ejecutar("clave-equivocada")
        // Assert
        assertEquals(ResultadoEliminacion.ConfirmacionInvalida, resultado)
        assertEquals(sesion, repo.actual)
        assertEquals(0, prefs.limpiezas)
    }

    @Test fun `sin sesion no llama a la API`() = runTest {
        // Arrange
        val api = ApiCuentaDoble()
        val caso = EliminarCuentaUseCase(api, SesionMemoria(), LimpiezaLocalSesion(SesionMemoria(), PreferenciasMemoria(), RecorridosMemoria()))
        // Act
        caso.ejecutar("una-clave-larga")
        // Assert
        assertEquals(0, api.llamadas)
    }
}
