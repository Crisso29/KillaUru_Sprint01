package pe.killauru.app.actualizador.ui

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.EstadoDescarga
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** FR-041, FR-043, RNF-15 (prototipo «Mapa sin conexión», paso 4): progreso y «Usar datos móviles». Patrón AAA. */
@OptIn(ExperimentalCoroutinesApi::class)
class DescargaViewModelTest {
    private class DescargaDoble : DescargaMapa {
        override val estado = MutableStateFlow<EstadoDescarga>(EstadoDescarga.SinEmpezar)
        val pedidos = mutableListOf<Pair<Boolean, Boolean>>() // (usarDatosMoviles, reemplazar)
        override fun programar(usarDatosMoviles: Boolean, reemplazar: Boolean) { pedidos += usarDatosMoviles to reemplazar }
    }

    @Before fun setUp() = Dispatchers.setMain(UnconfinedTestDispatcher())

    @After fun tearDown() = Dispatchers.resetMain()

    @Test fun `al abrir pide la descarga con la preferencia guardada y sin cortar una que ya corre`() {
        // Arrange
        val descarga = DescargaDoble()
        // Act
        DescargaViewModel(descarga, PreferenciasMemoria().apply { usarDatosMoviles = true })
        // Assert
        assertEquals(listOf(true to false), descarga.pedidos)
    }

    @Test fun `muestra el porcentaje y el peso que informa la descarga`() = runTest {
        // Arrange
        val descarga = DescargaDoble()
        val vm = DescargaViewModel(descarga, PreferenciasMemoria())
        backgroundScope.launch(UnconfinedTestDispatcher(testScheduler)) { vm.estado.collect {} }
        // Act
        descarga.estado.value = EstadoDescarga.Descargando(62, 2_400_000)
        // Assert
        assertEquals(EstadoDescarga.Descargando(62, 2_400_000), vm.estado.value.descarga)
        assertEquals(false, vm.estado.value.usarDatosMoviles)
    }

    @Test fun `permitir datos moviles lo guarda y reprograma la descarga sin perder lo bajado`() = runTest {
        // Arrange
        val descarga = DescargaDoble()
        val prefs = PreferenciasMemoria()
        val vm = DescargaViewModel(descarga, prefs)
        backgroundScope.launch(UnconfinedTestDispatcher(testScheduler)) { vm.estado.collect {} }
        // Act
        vm.cambiarDatosMoviles(true)
        // Assert — reemplazar: el trabajo nuevo sigue del `.parcial`
        assertTrue(prefs.usarDatosMoviles)
        assertEquals(true to true, descarga.pedidos.last())
        assertEquals(true, vm.estado.value.usarDatosMoviles)
    }

    @Test fun `descargar ahora vuelve a pedir la descarga aunque hubiera una esperando`() {
        // Arrange
        val descarga = DescargaDoble()
        val vm = DescargaViewModel(descarga, PreferenciasMemoria())
        // Act
        vm.descargarAhora()
        // Assert — reemplaza la que esperaba Wi-Fi o un reintento (sigue del parcial)
        assertEquals(false to true, descarga.pedidos.last())
    }
}
