package pe.killauru.app.sesion.ui

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.UnconfinedTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Before
import org.junit.Test
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sesion.dominio.casos.AceptarVersionesUseCase
import pe.killauru.app.sesion.dominio.casos.RevisarVersionesLegalesUseCase
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.assertEquals
import kotlin.test.assertIs

/**
 * FR-001, FR-004, FR-006, R-024: el flujo del primer uso decide qué pantalla toca según lo que ya está guardado.
 * La bienvenida es solo para quien nunca aceptó; una versión nueva de los términos se pide sin repetir el resto.
 */
@OptIn(ExperimentalCoroutinesApi::class)
class PrimerUsoViewModelTest {
    private val prefs = PreferenciasMemoria()

    @Before fun setUp() = Dispatchers.setMain(UnconfinedTestDispatcher())

    @After fun tearDown() = Dispatchers.resetMain()

    private fun crear() = PrimerUsoViewModel(
        RevisarVersionesLegalesUseCase(prefs, VIGENTE_T, VIGENTE_P, "texto t", "texto p"),
        AceptarVersionesUseCase(prefs) { 1_000L },
        prefs,
    )

    private fun aceptadoTodo() {
        prefs.terminosVersion = VIGENTE_T
        prefs.politicaVersion = VIGENTE_P
        prefs.categoria = CategoriaVehiculo.LIVIANO
    }

    @Test fun `la primera vez empieza por la bienvenida y luego los terminos`() {
        // Arrange
        val vm = crear()
        // Act
        val inicial = vm.estado.value
        vm.comenzar()
        // Assert
        assertEquals(EstadoPrimerUso.Bienvenida, inicial)
        assertIs<EstadoPrimerUso.MostrarTerminos>(vm.estado.value)
    }

    @Test fun `con todo aceptado y vehiculo elegido el primer uso ya esta completo`() {
        // Arrange
        aceptadoTodo()
        // Act
        val vm = crear()
        // Assert
        assertEquals(EstadoPrimerUso.Completado, vm.estado.value)
    }

    /** R-024: quien ya usó la app no ve otra vez la bienvenida, solo los términos nuevos. */
    @Test fun `si cambio la version de los terminos los pide sin bienvenida`() {
        // Arrange
        aceptadoTodo()
        prefs.terminosVersion = "t-anterior"
        // Act
        val vm = crear()
        // Assert
        assertIs<EstadoPrimerUso.MostrarTerminos>(vm.estado.value)
    }

    @Test fun `aceptar una version nueva no vuelve a pedir el vehiculo`() {
        // Arrange
        aceptadoTodo()
        prefs.terminosVersion = "t-anterior"
        val vm = crear()
        // Act
        vm.aceptarTerminos(VIGENTE_T, VIGENTE_P)
        // Assert
        assertEquals(EstadoPrimerUso.Completado, vm.estado.value)
        assertEquals(VIGENTE_T, prefs.terminosVersion)
    }

    /** Si la app se cerró después de aceptar y antes de elegir vehículo, retoma ahí (RN-18 lo necesita). */
    @Test fun `aceptado pero sin vehiculo retoma en el vehiculo`() {
        // Arrange
        aceptadoTodo()
        prefs.categoria = null
        // Act
        val vm = crear()
        // Assert
        assertEquals(EstadoPrimerUso.SeleccionarVehiculo, vm.estado.value)
    }

    @Test fun `el recorrido completo pasa por vehiculo permisos y cuenta`() {
        // Arrange
        val vm = crear()
        vm.comenzar()
        // Act
        vm.aceptarTerminos(VIGENTE_T, VIGENTE_P)
        val trasTerminos = vm.estado.value
        vm.seleccionarVehiculo(CategoriaVehiculo.LIVIANO)
        val trasVehiculo = vm.estado.value
        vm.permisosListos()
        val trasPermisos = vm.estado.value
        vm.completar()
        // Assert
        assertEquals(EstadoPrimerUso.SeleccionarVehiculo, trasTerminos)
        assertEquals(EstadoPrimerUso.SolicitarPermisos, trasVehiculo)
        assertEquals(EstadoPrimerUso.CuentaONo, trasPermisos)
        assertEquals(EstadoPrimerUso.Completado, vm.estado.value)
        assertEquals(CategoriaVehiculo.LIVIANO, prefs.categoria)
    }

    /** Prototipo: cada paso tiene «atrás». Retroceder no borra lo ya guardado; solo cambia la pantalla. */
    @Test fun `atras recorre los pasos al reves hasta la bienvenida`() {
        // Arrange
        val vm = crear()
        vm.comenzar()
        vm.aceptarTerminos(VIGENTE_T, VIGENTE_P)
        vm.seleccionarVehiculo(CategoriaVehiculo.LIVIANO)
        vm.permisosListos()
        // Act
        val pasos = List(4) { vm.volver(); vm.estado.value }
        // Assert
        assertEquals(EstadoPrimerUso.SolicitarPermisos, pasos[0])
        assertEquals(EstadoPrimerUso.SeleccionarVehiculo, pasos[1])
        assertIs<EstadoPrimerUso.MostrarTerminos>(pasos[2])
        assertEquals(EstadoPrimerUso.Bienvenida, pasos[3])
    }

    @Test fun `con una version nueva de los terminos atras no lleva a la bienvenida`() {
        // Arrange
        aceptadoTodo()
        prefs.terminosVersion = "t-anterior"
        val vm = crear()
        // Act
        vm.volver()
        // Assert
        assertIs<EstadoPrimerUso.MostrarTerminos>(vm.estado.value)
    }

    private companion object {
        const val VIGENTE_T = "t-2026.1"
        const val VIGENTE_P = "p-2026.1"
    }
}
