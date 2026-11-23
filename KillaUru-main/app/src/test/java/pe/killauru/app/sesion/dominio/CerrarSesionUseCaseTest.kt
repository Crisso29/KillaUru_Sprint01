package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.historial.dominio.ViajeRecorrido
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.mapa.dominio.RecortarAlDemoUseCase
import pe.killauru.app.mapa.dominio.RepositorioAnomaliasMemoria
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
 * T107 / FR-051, FR-066, FR-069, RN-21 d, R-023: cerrar sesión limpia el celular al instante (tokens, permiso, historial y
 * lo que el mapa tenía fuera del tramo demo) y
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
        val anomalias = RepositorioAnomaliasMemoria().apply { anomalias += listOf(anomalia("demo", true), anomalia("ruta", false)) }
        val pendiente = RevocacionPendienteMemoria()
        val caso = CerrarSesionUseCase(api, sesiones, LimpiezaLocalSesion(sesiones, prefs, recorridos, RecortarAlDemoUseCase(anomalias)), pendiente)
    }

    private companion object {
        fun anomalia(id: String, enDemo: Boolean) = AnomaliaLocal(id, TipoAnomalia.Bache, 3, false, Coordenada(-13.3, -74.4), 290.0,
            null, 0L, enDemo)
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
        assertEquals(listOf("demo"), e.anomalias.anomalias.map { it.id }, "FR-051: el mapa vuelve a solo el tramo demo")
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
