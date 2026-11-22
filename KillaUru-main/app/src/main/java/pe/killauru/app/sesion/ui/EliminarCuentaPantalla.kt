package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.width
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.CampoFormulario
import pe.killauru.app.comun.ui.FilaCasilla
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.TipoCampo
import pe.killauru.app.comun.ui.TonoAviso
import pe.killauru.app.comun.ui.Vinetas

/** Qué se borra al eliminar la cuenta (prototipo «Eliminar mi cuenta»). */
internal val LINEAS_ELIMINAR = listOf(
    "Borramos tu correo y tu contraseña, y cerramos tus sesiones.",
    "Borramos tu historial de este celular.",
    "Los baches que ayudaste a detectar siguen en el mapa: nunca estuvieron unidos a ti.",
)

/**
 * FR-067, HU-23 (prototipo «Eliminar mi cuenta»): no se puede deshacer, así que pide la contraseña y marcar «Entiendo»;
 * hasta entonces el botón rojo está deshabilitado.
 */
@Composable
internal fun EliminarCuentaPantalla(
    estado: EliminarUiState,
    alEliminar: (contrasena: String) -> Unit,
    alCancelar: () -> Unit,
) {
    var contrasena by rememberSaveable { mutableStateOf("") }
    var entiendo by rememberSaveable { mutableStateOf(false) }
    PantallaApp(
        titulo = "Eliminar mi cuenta",
        alVolver = alCancelar,
        pie = { PieEliminar(estado, { alEliminar(contrasena) }, entiendo && contrasena.isNotBlank(), alCancelar) },
    ) {
        Text("No se puede deshacer", style = MaterialTheme.typography.titleLarge, modifier = Modifier.semantics { heading() })
        Vinetas(LINEAS_ELIMINAR)
        estado.error?.let { Aviso("warn", TonoAviso.MAL, it) }
        CampoFormulario("Contraseña", contrasena, { contrasena = it }, TipoCampo.CONTRASENA)
        FilaCasilla(entiendo, { entiendo = it }) {
            Spacer(Modifier.width(8.dp))
            Text("Entiendo que no se puede deshacer")
        }
    }
}

/** Botón rojo, habilitado solo con contraseña y «Entiendo» marcados, y «Cancelar». */
@Composable
private fun PieEliminar(estado: EliminarUiState, alEliminar: () -> Unit, confirmado: Boolean, alCancelar: () -> Unit) {
    BotonPrincipal(if (estado.cargando) "Eliminando…" else "Eliminar mi cuenta", alEliminar,
        habilitado = !estado.cargando && confirmado, peligro = true)
    TextButton(onClick = alCancelar, modifier = Modifier.fillMaxWidth()) { Text("Cancelar", fontWeight = FontWeight.Bold) }
}

/** Une [EliminarCuentaPantalla] con su ViewModel; eliminada la cuenta, avisa a la navegación. */
@Composable
internal fun EliminarCuentaRuta(alEliminada: () -> Unit, alCancelar: () -> Unit, vm: EliminarCuentaViewModel = hiltViewModel()) {
    val estado by vm.estado.collectAsState()
    EliminarCuentaPantalla(estado, { vm.eliminar(it, alEliminada) }, alCancelar)
}
