package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.casos.*
import pe.killauru.app.soporte.*
import kotlin.test.*

class CicloSesionTest {
    private val sesion = Sesion("u1", "token", 1000, null)

    @Test fun `login guarda exitos y conserva la sesion ante error`() = runTest {
        val repo = SesionMemoria()
        val api = ApiCuentaDoble(ResultadoCuenta.Exitoso(sesion))
        val caso = IniciarSesionUseCase(api, repo)
        assertIs<ResultadoCuenta.Exitoso>(caso.ejecutar("a@b.pe", "clave"))
        assertEquals(sesion, repo.actual)
        api.resultado = ResultadoCuenta.CredencialesInvalidas
        assertIs<ResultadoCuenta.CredencialesInvalidas>(caso.ejecutar("a@b.pe", "mal"))
        assertEquals(sesion, repo.actual)
    }

    @Test fun `cierre e invalidacion limpian identidad`() = runTest {
        val repo = SesionMemoria(sesion)
        val prefs = PreferenciasMemoria()
        CerrarSesionUseCase(repo, prefs).ejecutar()
        assertNull(repo.actual)
        assertEquals(1, prefs.limpiezas)
        repo.guardar(sesion)
        AplicarSesionInvalidaUseCase(repo, LimpiezaLocalSesion(repo, prefs)).ejecutar()
        assertNull(repo.actual)
        assertEquals(2, prefs.limpiezas)
    }

    @Test fun `eliminar requiere sesion y solo limpia si servidor acepta`() = runTest {
        val repo = SesionMemoria()
        val prefs = PreferenciasMemoria()
        val api = ApiCuentaDoble(ResultadoCuenta.Exitoso(sesion))
        val caso = EliminarCuentaUseCase(api, repo, LimpiezaLocalSesion(repo, prefs))
        assertIs<ResultadoCuenta.Error>(caso.ejecutar("clave"))
        assertEquals(0, api.eliminaciones)
        repo.guardar(sesion)
        api.elimina = false
        assertIs<ResultadoCuenta.Error>(caso.ejecutar("clave"))
        assertEquals(sesion, repo.actual)
        api.elimina = true
        assertIs<ResultadoCuenta.Exitoso>(caso.ejecutar("clave"))
        assertNull(repo.actual)
        assertEquals(1, prefs.limpiezas)
    }

    @Test fun `registro y login propagan el resultado del servidor`() = runTest {
        val api = ApiCuentaDoble(ResultadoCuenta.Exitoso(sesion))
        val repo = SesionMemoria()
        assertIs<ResultadoCuenta.Exitoso>(RegistrarCuentaUseCase(api).ejecutar("a@b.pe", "ClaveSegura1!"))
        assertIs<ResultadoCuenta.Exitoso>(RegistrarseUseCase(api, repo).ejecutar("a@b.pe", "ClaveSegura1!"))
        assertEquals(sesion, repo.actual)
        api.resultado = ResultadoCuenta.CredencialesInvalidas
        assertIs<ResultadoCuenta.CredencialesInvalidas>(RegistrarseUseCase(api, repo).ejecutar("a@b.pe", "ClaveSegura1!"))
        assertIs<ResultadoCuenta.CredencialesInvalidas>(LoginUseCase(api, repo).ejecutar("a@b.pe", "mal"))
    }

    @Test fun `consentimiento persiste ambas versiones con la misma hora`() = runTest {
        val prefs = PreferenciasMemoria()
        AceptarVersionesUseCase(prefs) { 1234 }.aceptar("2", "3")
        assertEquals("2", prefs.terminosVersion)
        assertEquals("3", prefs.politicaVersion)
        assertEquals(1234L, prefs.terminosAceptadoUtc)
        assertEquals(prefs.terminosAceptadoUtc, prefs.politicaAceptadoUtc)
    }
}
