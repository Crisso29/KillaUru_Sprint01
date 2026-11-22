package pe.killauru.app.sesion.ui

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import pe.killauru.app.sesion.dominio.ResultadoRecuperacion
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.sesion.dominio.casos.CerrarSesionUseCase
import pe.killauru.app.sesion.dominio.casos.EliminarCuentaUseCase
import pe.killauru.app.sesion.dominio.casos.IniciarSesionUseCase
import pe.killauru.app.sesion.dominio.casos.LimpiezaLocalSesion
import pe.killauru.app.sesion.dominio.casos.ObtenerMisDatosUseCase
import pe.killauru.app.sesion.dominio.casos.RecuperarContrasenaUseCase
import pe.killauru.app.sesion.dominio.casos.RegistrarCuentaUseCase
import pe.killauru.app.soporte.ApiCuentaDoble
import pe.killauru.app.soporte.EventosMemoria
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.RecorridosMemoria
import pe.killauru.app.soporte.RevocacionPendienteMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

/** FR-068 (perfil), HU-25 (recuperar) y FR-067 (eliminar): lo que ve el conductor en cada caso. Patrón AAA. */
@OptIn(ExperimentalCoroutinesApi::class)
class CuentaViewModelsTest {
    private val datos = MisDatos("conductor@ejemplo.pe", Rol.ConductorRegistrado, 0L, true, emptyList())

    @Before fun setUp() = Dispatchers.setMain(UnconfinedTestDispatcher())

    @After fun tearDown() = Dispatchers.resetMain()

    @Test fun `el perfil muestra los datos que guarda el servidor`() {
        // Arrange + Act
        val vm = PerfilViewModel(ObtenerMisDatosUseCase(ApiCuentaDoble(datos = datos)))
        // Assert
        assertEquals(datos, vm.estado.value.datos)
        assertNull(vm.estado.value.error)
    }

    @Test fun `sin datos el perfil lo explica y deja reintentar`() {
        // Arrange
        val api = ApiCuentaDoble(datos = null)
        val vm = PerfilViewModel(ObtenerMisDatosUseCase(api))
        // Act
        api.datos = datos
        vm.cargar()
        // Assert
        assertEquals(datos, vm.estado.value.datos)
    }

    @Test fun `recuperar confirma el envio sin decir si la cuenta existe`() {
        // Arrange
        val vm = OlvideContrasenaViewModel(RecuperarContrasenaUseCase(ApiCuentaDoble(resultadoRecuperacion = ResultadoRecuperacion.Enviado)))
        // Act
        vm.enviar("conductor@ejemplo.pe")
        // Assert
        assertTrue(vm.estado.value.enviado)
        assertNull(vm.estado.value.error)
    }

    @Test fun `recuperar con un correo mal escrito pide corregirlo`() {
        // Arrange
        val vm = OlvideContrasenaViewModel(RecuperarContrasenaUseCase(ApiCuentaDoble()))
        // Act
        vm.enviar("sin-arroba")
        // Assert
        assertFalse(vm.estado.value.enviado)
        assertEquals("Escribe un correo válido.", vm.estado.value.error)
    }

    @Test fun `eliminar con la contrasena equivocada lo dice y no sale de la pantalla`() {
        // Arrange — conductor con sesión que escribe mal su contraseña
        val sesiones = SesionMemoria().apply { actual = SesionesDePrueba.de(Rol.ConductorRegistrado) }
        val api = ApiCuentaDoble(resultadoEliminacion = ResultadoEliminacion.ConfirmacionInvalida)
        val vm = EliminarCuentaViewModel(EliminarCuentaUseCase(api, sesiones, LimpiezaLocalSesion(sesiones, PreferenciasMemoria(), RecorridosMemoria())))
        var salio = false
        // Act
        vm.eliminar("equivocada") { salio = true }
        // Assert
        assertEquals("La contraseña no es correcta.", vm.estado.value.error)
        assertFalse(salio)
    }

    @Test fun `cerrar sesion cuenta las detecciones que aun no se enviaron`() {
        // Arrange — 3 por enviar y 1 ya enviada
        val eventos = EventosMemoria().apply {
            repeat(3) { eventos.add(eventoDePrueba("p$it", EstadoLocal.PENDING)) }
            eventos.add(eventoDePrueba("e", EstadoLocal.SYNCED))
        }
        val sesiones = SesionMemoria()
        // Act
        val vm = CerrarSesionViewModel(
            CerrarSesionUseCase(ApiCuentaDoble(), sesiones, LimpiezaLocalSesion(sesiones, PreferenciasMemoria(), RecorridosMemoria()),
                RevocacionPendienteMemoria()),
            eventos,
        )
        // Assert
        assertEquals(3, vm.pendientes.value)
    }

    @Test fun `login con credenciales invalidas no dice cual fallo y avisa del limite`() {
        // Arrange
        val vm = LoginViewModel(IniciarSesionUseCase(ApiCuentaDoble(ResultadoCuenta.CredencialesInvalidas), SesionMemoria()))
        var entro = false
        // Act
        vm.iniciarSesion("conductor@ejemplo.pe", "equivocada") { entro = true }
        // Assert — el mismo texto exista o no el correo (RN-19)
        assertEquals("Correo o contraseña incorrectos. Si fallas varias veces, espera unos minutos.", vm.estado.value.error)
        assertFalse(entro)
    }

    @Test fun `ajustes sabe si hay cuenta para ofrecer perfil o crear cuenta`() {
        // Arrange
        val sesiones = SesionMemoria()
        val vm = AjustesViewModel(PreferenciasMemoria(), sesiones)
        assertFalse(vm.estado.value.conCuenta)
        // Act — el conductor inicia sesión y vuelve a Ajustes
        sesiones.actual = SesionesDePrueba.de(Rol.ConductorRegistrado)
        vm.recargar()
        // Assert
        assertTrue(vm.estado.value.conCuenta)
    }

    @Test fun `reenviar la verificacion se marca para no repetirla`() {
        // Arrange
        val vm = VerificarCorreoViewModel(ApiCuentaDoble())
        // Act
        vm.reenviar()
        // Assert
        assertTrue(vm.reenviado.value)
    }

    @Test fun `el registro muestra la version de privacidad que se acepto en el primer uso`() {
        // Arrange
        val prefs = PreferenciasMemoria().apply { politicaVersion = "1.2" }
        // Act
        val vm = RegistroViewModel(RegistrarCuentaUseCase(ApiCuentaDoble(), SesionMemoria(), prefs), prefs)
        // Assert — la misma versión que se enviará en RegistroSolicitud (FR-003)
        assertEquals("1.2", vm.estado.value.politicaVersion)
    }

    private fun eventoDePrueba(id: String, estado: EstadoLocal) = Evento(
        eventoId = id, detectadoUtc = Instant.EPOCH, lat = -13.0, lon = -74.0, precisionM = 5.0,
        tipo = TipoAnomalia.Bache, severidad = 2, confidence = 0.9, modeloVersion = "1", seudonimo = "s",
        estado = estado, estadoDesde = Instant.EPOCH,
    )
}
