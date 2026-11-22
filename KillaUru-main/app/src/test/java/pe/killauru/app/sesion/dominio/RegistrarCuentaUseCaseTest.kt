package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.casos.RegistrarCuentaUseCase
import pe.killauru.app.soporte.ApiCuentaDoble
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

/** T107 / FR-060, HU-22, HU-27, D13: registro con contraseña de 10+ y las versiones legales aceptadas. */
class RegistrarCuentaUseCaseTest {
    private val conductor = SesionesDePrueba.de(Rol.ConductorRegistrado)

    private fun preferenciasConConsentimiento() = PreferenciasMemoria().apply {
        politicaVersion = "1.2"
        terminosVersion = "1.3"
    }

    @Test fun `registro envia las versiones aceptadas y guarda la sesion del 201`() = runTest {
        // Arrange
        val api = ApiCuentaDoble(ResultadoCuenta.Exitoso(conductor))
        val repo = SesionMemoria()
        val caso = RegistrarCuentaUseCase(api, repo, preferenciasConConsentimiento())
        // Act
        val resultado = caso.ejecutar("conductor@ejemplo.pe", "una-clave-larga")
        // Assert
        assertIs<ResultadoCuenta.Exitoso>(resultado)
        assertEquals(SolicitudRegistro("conductor@ejemplo.pe", "una-clave-larga", "1.2", "1.3"), api.ultimaSolicitudRegistro)
        assertEquals(conductor, repo.actual)
    }

    /** D13: el mínimo es 10; se valida antes de gastar una petición (y del límite por IP del servidor). */
    @Test fun `contrasena de menos de 10 caracteres se rechaza sin llamar a la API`() = runTest {
        // Arrange
        val api = ApiCuentaDoble(ResultadoCuenta.Exitoso(conductor))
        val caso = RegistrarCuentaUseCase(api, SesionMemoria(), preferenciasConConsentimiento())
        // Act
        val resultado = caso.ejecutar("conductor@ejemplo.pe", "corta-123")
        // Assert
        assertEquals(ResultadoCuenta.ContrasenaInvalida, resultado)
        assertEquals(0, api.llamadas)
    }

    /** HU-22, HU-27: sin aceptar la política y los términos no hay registro. */
    @Test fun `sin consentimiento aceptado no se registra`() = runTest {
        // Arrange
        val api = ApiCuentaDoble(ResultadoCuenta.Exitoso(conductor))
        val caso = RegistrarCuentaUseCase(api, SesionMemoria(), PreferenciasMemoria())
        // Act
        val resultado = caso.ejecutar("conductor@ejemplo.pe", "una-clave-larga")
        // Assert
        assertEquals(ResultadoCuenta.SinConsentimiento, resultado)
        assertEquals(0, api.llamadas)
    }

    @Test fun `correo en uso se informa y no guarda sesion`() = runTest {
        // Arrange
        val repo = SesionMemoria()
        val caso = RegistrarCuentaUseCase(ApiCuentaDoble(ResultadoCuenta.CorreoEnUso), repo, preferenciasConConsentimiento())
        // Act
        val resultado = caso.ejecutar("conductor@ejemplo.pe", "una-clave-larga")
        // Assert
        assertEquals(ResultadoCuenta.CorreoEnUso, resultado)
        assertNull(repo.actual)
    }
}
