package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.Vinetas

/**
 * FR-069 (prototipo «Cerrar sesión»): explica qué pasa en este celular antes de confirmar. Las detecciones por enviar
 * no se pierden: se envían sin cuenta.
 */
@Composable
internal fun CerrarSesionPantalla(pendientes: Int, alConfirmar: () -> Unit, alCancelar: () -> Unit) {
    PantallaApp(
        titulo = "Cerrar sesión",
        alVolver = alCancelar,
        pie = {
            BotonPrincipal("Cerrar sesión", alConfirmar)
            TextButton(onClick = alCancelar, modifier = Modifier.fillMaxWidth()) { Text("Cancelar", fontWeight = FontWeight.Bold) }
        },
    ) {
        Text("Esto pasará en este celular", style = MaterialTheme.typography.titleLarge, modifier = Modifier.semantics { heading() })
        Vinetas(lineasCierreSesion(pendientes))
    }
}

/** Lo que se explica al cerrar sesión; la línea de pendientes solo aparece si hay alguna. */
internal fun lineasCierreSesion(pendientes: Int): List<String> = buildList {
    add("Verás solo el tramo **Ayacucho – Vinchos** y no recibirás alertas.")
    add("Tu historial de viajes **se borra**, para que nadie más lo vea.")
    when {
        pendientes == 1 -> add("Tu **detección por enviar no se pierde**: se enviará sin cuenta.")
        pendientes > 1 -> add("Tus **$pendientes detecciones por enviar no se pierden**: se enviarán sin cuenta.")
    }
}

/** Une [CerrarSesionPantalla] con su ViewModel; cerrada la sesión, avisa a la navegación. */
@Composable
internal fun CerrarSesionRuta(alCerrada: () -> Unit, alCancelar: () -> Unit, vm: CerrarSesionViewModel = hiltViewModel()) {
    val pendientes by vm.pendientes.collectAsState()
    CerrarSesionPantalla(pendientes, { vm.cerrarSesion(alCerrada) }, alCancelar)
}
