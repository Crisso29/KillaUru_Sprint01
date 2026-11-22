package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.historial.dominio.ViajeRecorrido
import pe.killauru.app.sesion.dominio.casos.CerrarSesionUseCase
import pe.killauru.app.sesion.dominio.casos.LimpiezaLocalSesion
import pe.killauru.app.soporte.ApiCuentaDoble
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.RecorridosMemoria
import pe.killauru.app.soporte.RevocacionPendienteMemoria
import pe.killauru.app.soporte.SesionMemoria
import pe.killauru.app.soporte.SesionesDePrueba
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * T107 / FR-066, FR-069, RN-21 d, R-023: cerrar sesión limpia el celular al instante (tokens, permiso e historial) y
 * revoca en el servidor; sin red, deja la revocación programada. Patrón AAA.
 */
class CerrarSesionUseCaseTest {
    private val sesion = SesionesDePrueba.de(Rol.ConductorRegistrado)
    private val viaje = ViajeRecorrido("1", 0L, 1L, 0.0, 10.0, 2)

    private class Escenario(revocaConExito: Boolean, sesion: Sesion?, viaje: ViajeRecorrido) {
        val api = ApiCuentaDoble().apply { revocacionExitosa = revocaConExito }
        val sesiones = SesionMemoria(sesion)
        val prefs = PreferenciasMemoria()
        val recorridos = RecorridosMemoria(viaje)
        val pendiente = RevocacionPendienteMemoria()
        val caso = CerrarSesionUseCase(api, sesiones, LimpiezaLocalSesion(sesiones, prefs, recorridos), pendiente)
    }

    @Test fun `con red revoca en el servidor y borra lo local`() = runTest {
        // Arrange
        val e = Escenario(revocaConExito = true, sesion = sesion, viaje = viaje)
        // Act
        e.caso.ejecutar()
        // Assert
        assertEquals(sesion.tokenRenovacion, e.api.ultimaRevocacion)
        assertNull(e.sesiones.actual)
        assertEquals(1, e.prefs.limpiezas)
        assertTrue(e.recorridos.viajes.isEmpty(), "FR-069: el historial se borra al cerrar sesión")
        assertNull(e.pendiente.programada)
    }

    @Test fun `sin red borra lo local igual y deja la revocacion programada`() = runTest {
        // Arrange
        val e = Escenario(revocaConExito = false, sesion = sesion, viaje = viaje)
        // Act
        e.caso.ejecutar()
        // Assert — RN-21 d no espera a la red; R-023 revoca después
        assertNull(e.sesiones.actual)
        assertTrue(e.recorridos.viajes.isEmpty())
        assertEquals(sesion.tokenRenovacion, e.pendiente.programada)
    }

    @Test fun `sin sesion solo limpia y no llama al servidor`() = runTest {
        // Arrange
        val e = Escenario(revocaConExito = true, sesion = null, viaje = viaje)
        // Act
        e.caso.ejecutar()
        // Assert
        assertEquals(0, e.api.llamadas)
        assertTrue(e.recorridos.viajes.isEmpty())
        assertNull(e.pendiente.programada)
    }
}
