package pe.killauru.app.comun.dominio

import kotlinx.coroutines.flow.emptyFlow
import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.EstadoDescarga
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sincronizacion.dominio.ProgramadorSincronizacion
import pe.killauru.app.sincronizacion.dominio.RecuperarEnviandoUseCase
import pe.killauru.app.soporte.EventosMemoria
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.assertEquals

/**
 * T072 / FR-021, FR-022, FR-033, FR-043: al abrir la app se recuperan los envíos cortados **antes** de programar la
 * sincronización, y el mapa se comprueba solo si ya pasó el primer uso (allí lo pide el paso 4). Patrón AAA.
 */
class IniciarAppTest {
    private val orden = mutableListOf<String>()

    private val eventos = object : pe.killauru.app.deteccion.dominio.RepositorioEventos by EventosMemoria() {
        override suspend fun recuperarEnviando() { orden += "recuperar" }
    }
    private val sync = object : ProgramadorSincronizacion {
        override fun programar() { orden += "sync" }
    }
    private val mapa = object : DescargaMapa {
        override val estado = emptyFlow<EstadoDescarga>()
        override fun programar(usarDatosMoviles: Boolean, reemplazar: Boolean) { orden += "mapa moviles=$usarDatosMoviles reemplazar=$reemplazar" }
    }

    @Test fun `recupera los envios cortados, luego programa la sincronizacion y comprueba el mapa sin cortar una descarga`() = runTest {
        // Arrange — primer uso hecho y datos móviles permitidos
        val prefs = PreferenciasMemoria().apply { categoria = CategoriaVehiculo.LIVIANO; usarDatosMoviles = true }
        // Act
        IniciarApp(RecuperarEnviandoUseCase(eventos), sync, mapa, prefs).ejecutar()
        // Assert
        assertEquals(listOf("recuperar", "sync", "mapa moviles=true reemplazar=false"), orden)
    }

    @Test fun `antes del primer uso no pide el mapa`() = runTest {
        // Arrange
        val prefs = PreferenciasMemoria()
        // Act
        IniciarApp(RecuperarEnviandoUseCase(eventos), sync, mapa, prefs).ejecutar()
        // Assert
        assertEquals(listOf("recuperar", "sync"), orden)
    }
}
