package pe.killauru.app.sesion.ui

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.actualizador.ui.MapaSinConexionRuta
import pe.killauru.app.comun.ui.TrasPrimerUso

/**
 * FR-001, FR-004, FR-006: muestra la pantalla de cada paso del primer uso; al completarlo avisa a NavGrafo. La flecha
 * del encabezado y el botón Atrás de Android hacen lo mismo: retroceder un paso (`PrimerUsoViewModel.volver`).
 */
@Composable
internal fun PrimerUsoRuta(alCompletar: (TrasPrimerUso) -> Unit, viewModel: PrimerUsoViewModel = hiltViewModel()) {
    val estado by viewModel.estado.collectAsState()
    var tras by rememberSaveable { mutableStateOf(TrasPrimerUso.MAPA) }
    val puedeVolver = viewModel.puedeVolver(estado)
    BackHandler(enabled = puedeVolver) { viewModel.volver() }
    val atras: (() -> Unit)? = if (puedeVolver) viewModel::volver else null
    when (estado) {
        EstadoPrimerUso.CuentaONo -> CuentaONoRuta(atras) { eleccion -> tras = eleccion; viewModel.completar() }
        EstadoPrimerUso.Completado -> LaunchedEffect(Unit) { alCompletar(tras) }
        else -> PasoPrevio(estado, viewModel, atras)
    }
}

/** Bienvenida, términos, vehículo y permisos: cada uno con su pantalla. */
@Composable
private fun PasoPrevio(paso: EstadoPrimerUso, viewModel: PrimerUsoViewModel, atras: (() -> Unit)?) {
    when (paso) {
        EstadoPrimerUso.Bienvenida -> BienvenidaPantalla(onComenzar = viewModel::comenzar)
        is EstadoPrimerUso.MostrarTerminos -> TerminosPantalla(
            paso.terminosTexto, paso.politicaTexto, paso.terminosVersion,
            onAceptar = { viewModel.aceptarTerminos(paso.terminosVersion, paso.politicaVersion) },
            alVolver = atras,
        )
        EstadoPrimerUso.SeleccionarVehiculo -> VehiculoPantalla(onSeleccionar = viewModel::seleccionarVehiculo, alVolver = atras)
        EstadoPrimerUso.SolicitarPermisos -> PermisosRuta(alContinuar = viewModel::permisosListos, alVolver = atras)
        EstadoPrimerUso.DescargarMapa -> MapaSinConexionRuta(alSeguir = viewModel::seguirTrasMapa, alVolver = viewModel::volver)
        else -> Box(Modifier.fillMaxSize(), Alignment.Center) { CircularProgressIndicator() } // Cargando
    }
}

/** El primer uso se guarda como completo en los tres casos; luego se abre el registro, el inicio de sesión o el mapa. */
@Composable
private fun CuentaONoRuta(atras: (() -> Unit)?, alElegir: (TrasPrimerUso) -> Unit) = CuentaONoPantalla(
    onCrearCuenta = { alElegir(TrasPrimerUso.CREAR_CUENTA) },
    onContinuarSinCuenta = { alElegir(TrasPrimerUso.MAPA) },
    onYaTengoCuenta = { alElegir(TrasPrimerUso.INICIAR_SESION) },
    alVolver = atras,
)
