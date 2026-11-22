package pe.killauru.app.sesion.dominio.casos

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.ResultadoRecuperacion
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.soporte.ApiCuentaDoble
import kotlin.test.assertEquals

/** HU-25 (recuperar) y FR-068 (mis datos). Patrón AAA. */
class CasosCuentaNuevosTest {
    @Test fun `un correo sin arroba no llega a la red`() = runTest {
        // Arrange
        val api = ApiCuentaDoble()
        // Act
        val r = RecuperarContrasenaUseCase(api).ejecutar("  conductor.ejemplo.pe ")
        // Assert — no se gasta una petición ni el límite de intentos
        assertEquals(ResultadoRecuperacion.CorreoInvalido, r)
        assertEquals(0, api.llamadas)
    }

    @Test fun `un correo valido se envia sin espacios`() = runTest {
        // Arrange
        val api = ApiCuentaDoble(resultadoRecuperacion = ResultadoRecuperacion.Enviado)
        // Act
        val r = RecuperarContrasenaUseCase(api).ejecutar(" conductor@ejemplo.pe ")
        // Assert
        assertEquals(ResultadoRecuperacion.Enviado, r)
        assertEquals(1, api.llamadas)
    }

    @Test fun `mis datos son los que devuelve la api`() = runTest {
        // Arrange
        val datos = MisDatos("conductor@ejemplo.pe", Rol.ConductorRegistrado, 0L, true, emptyList())
        // Act
        val r = ObtenerMisDatosUseCase(ApiCuentaDoble(datos = datos)).ejecutar()
        // Assert
        assertEquals(datos, r)
    }
}
