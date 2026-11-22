package pe.killauru.app.sesion.ui

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.hilt.navigation.compose.hiltViewModel

/**
 * FR-001, FR-004, FR-006: muestra la pantalla de cada paso del primer uso; al completarlo avisa a NavGrafo. La flecha
 * del encabezado y el botón Atrás de Android hacen lo mismo: retroceder un paso (`PrimerUsoViewModel.volver`).
 */
@Composable
internal fun PrimerUsoRuta(alCompletar: () -> Unit, viewModel: PrimerUsoViewModel = hiltViewModel()) {
    val estado by viewModel.estado.collectAsState()
    val puedeVolver = viewModel.puedeVolver(estado)
    BackHandler(enabled = puedeVolver) { viewModel.volver() }
    val atras: (() -> Unit)? = if (puedeVolver) viewModel::volver else null
    when (val paso = estado) {
        EstadoPrimerUso.Cargando -> Box(Modifier.fillMaxSize(), Alignment.Center) { CircularProgressIndicator() }
        EstadoPrimerUso.Bienvenida -> BienvenidaPantalla(onComenzar = viewModel::comenzar)
        is EstadoPrimerUso.MostrarTerminos -> TerminosPantalla(
            paso.terminosTexto, paso.politicaTexto, paso.terminosVersion,
            onAceptar = { viewModel.aceptarTerminos(paso.terminosVersion, paso.politicaVersion) },
            alVolver = atras,
        )
        EstadoPrimerUso.SeleccionarVehiculo -> VehiculoPantalla(onSeleccionar = viewModel::seleccionarVehiculo, alVolver = atras)
        EstadoPrimerUso.SolicitarPermisos -> PermisosRuta(alContinuar = viewModel::permisosListos, alVolver = atras)
        // Parte 2 de T024: «Crear cuenta» llevará al registro y «Ya tengo cuenta» al inicio de sesión.
        EstadoPrimerUso.CuentaONo -> CuentaONoPantalla(
            onCrearCuenta = viewModel::completar,
            onContinuarSinCuenta = viewModel::completar,
            alVolver = atras,
        )
        EstadoPrimerUso.Completado -> LaunchedEffect(Unit) { alCompletar() }
    }
}
