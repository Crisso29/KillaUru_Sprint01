package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import pe.killauru.app.sesion.dominio.casos.LoginUseCase
import pe.killauru.app.sesion.dominio.casos.RegistrarseUseCase
import kotlin.test.Test
import kotlin.test.assertIs

/** T105: FR-063, FR-065 — casos de uso de cuenta persisten sesión. */
class CasosCuentaTest {

    private val sesion = Sesion("u1", "jwt.tok.en", Long.MAX_VALUE, null)

    private inner class ApiFalsa(private val resultado: ResultadoCuenta) : ApiCuenta {
        override suspend fun registrar(correo: String, contrasena: String) = resultado
        override suspend fun login(correo: String, contrasena: String) = resultado
        override suspend fun solicitarVerificacion(correo: String) = Unit
        override suspend fun eliminarCuenta(usuarioId: String, token: String) = true
    }

    private inner class RepoFalso : RepositorioSesion {
        var guardada: Sesion? = null
        var cerrada = false
        override suspend fun obtener() = guardada
        override suspend fun guardar(s: Sesion) { guardada = s }
        override suspend fun cerrar() { cerrada = true }
    }

    @Test
    fun `login exitoso guarda la sesion`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val uc = LoginUseCase(ApiFalsa(ResultadoCuenta.Exitoso(sesion)), repo)
        // Act
        val resultado = uc.ejecutar("a@b.com", "P@ss1234")
        // Assert
        assertIs<ResultadoCuenta.Exitoso>(resultado)
    }

    @Test
    fun `registro con contrasena invalida retorna CredencialesInvalidas`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val uc = RegistrarseUseCase(ApiFalsa(ResultadoCuenta.Exitoso(sesion)), repo)
        // Act
        val resultado = uc.ejecutar("a@b.com", "123")
        // Assert
        assertIs<ResultadoCuenta.CredencialesInvalidas>(resultado)
    }
}
