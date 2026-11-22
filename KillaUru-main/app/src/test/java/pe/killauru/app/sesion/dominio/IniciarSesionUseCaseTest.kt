package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.casos.IniciarSesionUseCase
import pe.killauru.app.soporte.ApiCuentaDoble
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

/** T107 / FR-061, FR-062, R-033: la app solo guarda sesiones de conductor; el 401 no revela la causa. */
class IniciarSesionUseCaseTest {

    @Test fun `conductor registrado inicia sesion y la guarda`() = runTest {
        // Arrange
        val conductor = SesionesDePrueba.de(Rol.ConductorRegistrado)
        val repo = SesionMemoria()
        val caso = IniciarSesionUseCase(ApiCuentaDoble(ResultadoCuenta.Exitoso(conductor)), repo)
        // Act
        val resultado = caso.ejecutar("conductor@ejemplo.pe", "una-clave-larga")
        // Assert
        assertIs<ResultadoCuenta.Exitoso>(resultado)
        assertEquals(conductor, repo.actual)
    }

    /** R-033, FR-062: «Esta cuenta es de administración; usa el panel». La sesión no se guarda. */
    @Test fun `cuenta de administracion se rechaza y no se guarda`() = runTest {
        // Arrange
        val repo = SesionMemoria()
        val caso = IniciarSesionUseCase(ApiCuentaDoble(ResultadoCuenta.Exitoso(SesionesDePrueba.de(Rol.Administrador))), repo)
        // Act
        val resultado = caso.ejecutar("admin@ejemplo.pe", "una-clave-larga")
        // Assert
        assertEquals(ResultadoCuenta.CuentaDeAdministracion, resultado)
        assertNull(repo.actual)
    }

    @Test fun `credenciales invalidas no tocan la sesion guardada`() = runTest {
        // Arrange
        val anterior = SesionesDePrueba.de(Rol.ConductorRegistrado)
        val repo = SesionMemoria(anterior)
        val caso = IniciarSesionUseCase(ApiCuentaDoble(ResultadoCuenta.CredencialesInvalidas), repo)
        // Act
        val resultado = caso.ejecutar("conductor@ejemplo.pe", "mal")
        // Assert
        assertEquals(ResultadoCuenta.CredencialesInvalidas, resultado)
        assertEquals(anterior, repo.actual)
    }
}
