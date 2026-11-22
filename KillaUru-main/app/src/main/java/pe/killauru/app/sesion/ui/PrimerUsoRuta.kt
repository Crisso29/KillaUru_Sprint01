package pe.killauru.app.sesion.ui

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

/** FR-001, FR-004, FR-006: muestra la pantalla de cada paso del primer uso; al completarlo avisa a NavGrafo. */
@Composable
internal fun PrimerUsoRuta(alCompletar: () -> Unit, viewModel: PrimerUsoViewModel = hiltViewModel()) {
    val estado by viewModel.estado.collectAsState()
    when (val paso = estado) {
        EstadoPrimerUso.Cargando -> Box(Modifier.fillMaxSize(), Alignment.Center) { CircularProgressIndicator() }
        EstadoPrimerUso.Bienvenida -> BienvenidaPantalla(onComenzar = viewModel::comenzar)
        is EstadoPrimerUso.MostrarTerminos -> TerminosPantalla(paso.terminosTexto, paso.politicaTexto) {
            viewModel.aceptarTerminos(paso.terminosVersion, paso.politicaVersion)
        }
        EstadoPrimerUso.SeleccionarVehiculo -> VehiculoPantalla(onSeleccionar = viewModel::seleccionarVehiculo)
        EstadoPrimerUso.SolicitarPermisos -> PermisosRuta(alContinuar = viewModel::permisosListos)
        // Parte 2 de T024: «Crear cuenta» llevará al registro; mientras tanto ambas opciones continúan sin cuenta.
        EstadoPrimerUso.CuentaONo -> CuentaONoPantalla(
            onCrearCuenta = viewModel::completar,
            onContinuarSinCuenta = viewModel::completar,
        )
        EstadoPrimerUso.Completado -> LaunchedEffect(Unit) { alCompletar() }
    }
}
