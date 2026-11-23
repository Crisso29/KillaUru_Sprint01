package pe.killauru.app.mapa.ui

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.R
import pe.killauru.app.actualizador.ui.DescargaViewModel
import pe.killauru.app.actualizador.ui.PantallaDescargaUi
import pe.killauru.app.actualizador.ui.TextosDescarga
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.mapa.dominio.EstadoDescarga

/**
 * FR-041, FR-043 (prototipo 13, «Aún no tienes el mapa»): dice cuánto pesa, que la detección sigue mientras tanto y
 * deja descargar ya o permitir los datos móviles. Mientras descarga muestra el avance en lugar del botón.
 */
@Composable
internal fun SinMapaPantalla(ui: PantallaDescargaUi, alDescargar: () -> Unit, alUsarMoviles: () -> Unit) {
    val peso = TextosDescarga.peso(TextosDescarga.bytes(ui.descarga)).let { if (it.isEmpty()) "" else " ($it)" }
    Column(
        Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(21.dp),
        verticalArrangement = Arrangement.spacedBy(13.dp, Alignment.CenterVertically),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Image(painterResource(R.drawable.ilustracion_sin_mapa), contentDescription = null)
        Text("Aún no tienes el mapa", style = MaterialTheme.typography.headlineMedium, modifier = Modifier.semantics { heading() })
        Text("Conéctate a Wi-Fi para descargarlo$peso. Mientras tanto la detección sigue guardando lo que encuentra.",
            textAlign = TextAlign.Center, color = MaterialTheme.colorScheme.onSurfaceVariant)
        AccionesDescarga(ui, alDescargar, alUsarMoviles)
    }
}

@Composable
private fun AccionesDescarga(ui: PantallaDescargaUi, alDescargar: () -> Unit, alUsarMoviles: () -> Unit) {
    val descarga = ui.descarga
    if (descarga is EstadoDescarga.Descargando) {
        Text("Descargando… ${descarga.porcentaje} %", fontWeight = FontWeight.Bold)
        LinearProgressIndicator(progress = { descarga.porcentaje / 100f }, modifier = Modifier.fillMaxWidth())
    } else {
        BotonPrincipal("Descargar ahora", alDescargar)
    }
    if (!ui.usarDatosMoviles) {
        TextButton(onClick = alUsarMoviles) { Text("Usar datos móviles", fontWeight = FontWeight.Bold) }
    }
}

/** Une [SinMapaPantalla] con la descarga; al terminar, [alListo] hace que la pestaña muestre el mapa. */
@Composable
internal fun SinMapaRuta(alListo: () -> Unit, vm: DescargaViewModel = hiltViewModel()) {
    val ui by vm.estado.collectAsState()
    LaunchedEffect(ui.descarga) { if (ui.descarga == EstadoDescarga.Listo) alListo() }
    SinMapaPantalla(ui, vm::descargarAhora) { vm.cambiarDatosMoviles(true) }
}
