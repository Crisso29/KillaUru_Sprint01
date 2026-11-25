package pe.killauru.app.mapa.ui

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.corredor.dominio.Corredor
import pe.killauru.app.corredor.dominio.FuenteCorredor
import pe.killauru.app.corredor.dominio.Vertice
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.flow.MutableStateFlow
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.EstadoDescarga
import pe.killauru.app.mapa.dominio.FuenteEstiloMapa
import pe.killauru.app.mapa.dominio.FuenteZonaDemo
import pe.killauru.app.mapa.dominio.RepositorioAnomaliasMemoria
import pe.killauru.app.mapa.dominio.ZonaDemo
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertIs
import kotlin.test.assertTrue
import java.time.Duration

/** FR-040, T096: sin mapa descargado se pide descargarlo; con mapa, el estilo local y la vía. Patrón AAA. */
@OptIn(ExperimentalCoroutinesApi::class)
class MapaViewModelTest {
    private val reloj = object : Reloj {
        override fun ahoraConfiable(): Instant = Instant.EPOCH
        override fun monotonico() = Duration.ZERO
        override suspend fun anclarGps(utc: Instant, elapsed: Duration) = Unit
        override suspend fun anclarServidor(utc: Instant) = Unit
    }

    private fun estilo(json: String?) = object : FuenteEstiloMapa { override suspend fun estilo() = json }
    private fun corredor(c: Corredor?) = object : FuenteCorredor { override suspend fun corredor() = c }
    private fun zonaDemo(z: ZonaDemo?) = object : FuenteZonaDemo { override suspend fun zona() = z }

    private val descarga = object : DescargaMapa {
        override val estado = MutableStateFlow<EstadoDescarga>(EstadoDescarga.SinEmpezar)
        override fun programar(usarDatosMoviles: Boolean, reemplazar: Boolean) = Unit
    }

    @Before fun setUp() = Dispatchers.setMain(UnconfinedTestDispatcher())

    @After fun tearDown() = Dispatchers.resetMain()

    @Test fun `sin mapa descargado el estado lo dice`() {
        // Arrange + Act
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, estilo(null), corredor(null), descarga, zonaDemo(null))
        // Assert
        assertEquals(EstadoMapaUi.SinMapa, vm.mapa.value)
    }

    @Test fun `con mapa entrega el estilo local y la via como lon lat`() {
        // Arrange
        val via = Corredor(listOf(Vertice(lat = -13.33, lon = -74.50, km = 268.0), Vertice(lat = -13.15, lon = -74.21, km = 335.0)))
        // Act
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, estilo("""{"version":8}"""), corredor(via), descarga, zonaDemo(null))
        // Assert
        val mapa = assertIs<EstadoMapaUi.ConMapa>(vm.mapa.value)
        assertEquals("""{"version":8}""", mapa.estilo)
        assertEquals(listOf(-74.50 to -13.33, -74.21 to -13.15), mapa.via)
    }

    @Test fun `al terminar una descarga nueva el mapa se vuelve a cargar con la version nueva`() {
        // Arrange — había v1 y la CDN publica v2
        var version = "v1"
        val fuente = object : FuenteEstiloMapa { override suspend fun estilo() = """{"name":"$version"}""" }
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, fuente, corredor(null), descarga, zonaDemo(null))
        // Act
        version = "v2"
        descarga.estado.value = EstadoDescarga.Listo
        // Assert — sin reabrir la pestaña
        assertEquals("""{"name":"v2"}""", assertIs<EstadoMapaUi.ConMapa>(vm.mapa.value).estilo)
    }

    /** Carrera vista en el emulador: una lectura vieja (v1) que termina tarde no debe pisar a la nueva (v2). */
    @Test fun `una carga vieja que termina despues no pisa a la mas nueva`() {
        // Arrange — la primera lectura (v1) queda esperando; mientras, termina la descarga de v2
        val primera = CompletableDeferred<String>()
        var llamadas = 0
        val fuente = object : FuenteEstiloMapa {
            override suspend fun estilo() = if (++llamadas == 1) primera.await() else "v2"
        }
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, fuente, corredor(null), descarga, zonaDemo(null))
        descarga.estado.value = EstadoDescarga.Listo
        // Act — la lectura vieja termina al final
        primera.complete("v1")
        // Assert
        assertEquals("v2", assertIs<EstadoMapaUi.ConMapa>(vm.mapa.value).estilo)
    }

    /** T129 / FR-049: si FuenteZonaDemo devuelve vértices, tramosDemo los expone. */
    @Test fun `con zona cargada tramosDemo expone los vertices`() {
        // Arrange
        val vertices = listOf(Coordenada(-13.1, -74.2), Coordenada(-13.2, -74.3), Coordenada(-13.3, -74.4))
        val zona = ZonaDemo(vertices)
        // Act
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, estilo(null), corredor(null), descarga, zonaDemo(zona))
        // Assert
        assertEquals(vertices, vm.tramosDemo.value)
    }

    @Test fun `sin zona tramosDemo esta vacio`() {
        // Arrange + Act
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, estilo(null), corredor(null), descarga, zonaDemo(null))
        // Assert
        assertTrue(vm.tramosDemo.value.isEmpty())
    }

    @Test fun `recibirPosicionParaDemo fuera de zona activa el aviso una sola vez`() {
        // Arrange — zona de un solo punto; cualquier otro punto está fuera
        val dentro = Coordenada(-13.0, -74.0)
        val zona = ZonaDemo(listOf(dentro, Coordenada(-13.0, -74.1), Coordenada(-13.1, -74.1), Coordenada(-13.1, -74.0)))
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, estilo(null), corredor(null), descarga, zonaDemo(zona))
        val fuera = Coordenada(-14.0, -75.0)
        // Act
        vm.recibirPosicionParaDemo(fuera, Alcance.Anonimo)
        // Assert
        assertTrue(vm.avisoFueraDemo.value)
        // Segunda llamada no cambia estado (ya avisado)
        vm.recibirPosicionParaDemo(fuera, Alcance.Anonimo)
        assertTrue(vm.avisoFueraDemo.value)
    }

    @Test fun `descartarAvisoFueraDemo oculta el aviso`() {
        // Arrange
        val zona = ZonaDemo(listOf(Coordenada(-13.0, -74.0), Coordenada(-13.0, -74.1), Coordenada(-13.1, -74.1)))
        val vm = MapaViewModel(RepositorioAnomaliasMemoria(), reloj, estilo(null), corredor(null), descarga, zonaDemo(zona))
        vm.recibirPosicionParaDemo(Coordenada(-14.0, -75.0), Alcance.Anonimo)
        // Act
        vm.descartarAvisoFueraDemo()
        // Assert
        assertFalse(vm.avisoFueraDemo.value)
    }
}
