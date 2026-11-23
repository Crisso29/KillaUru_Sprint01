package pe.killauru.app.actualizador.ui

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.R
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.FilaAjuste
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.Paso
import pe.killauru.app.mapa.dominio.EstadoDescarga
import pe.killauru.app.sesion.ui.PASOS_PRIMER_USO

private const val LONGITUD_KM = 335 // eje canónico, 335,073 km (backend/db/datos/README.md)
private const val PASO_MAPA = 4

/**
 * FR-041, FR-043, RNF-15 (prototipo 05, «Mapa sin conexión», paso 4 de 5): la descarga sigue en segundo plano y el
 * conductor no tiene que esperarla; si se corta, continúa del parcial. Sin permiso usa solo Wi-Fi.
 */
@Composable
internal fun MapaSinConexionPantalla(
    ui: PantallaDescargaUi,
    alCambiarMoviles: (Boolean) -> Unit,
    alSeguir: () -> Unit,
    alVolver: (() -> Unit)? = null,
) = PantallaApp(
    titulo = "Mapa sin conexión",
    alVolver = alVolver,
    paso = Paso(PASO_MAPA, PASOS_PRIMER_USO),
    pie = { BotonPrincipal(if (ui.descarga == EstadoDescarga.Listo) "Continuar" else "Seguir mientras descarga", alSeguir) },
) {
    Image(painterResource(R.drawable.ilustracion_mapa_sin_conexion), contentDescription = null, contentScale = ContentScale.Crop,
        modifier = Modifier.fillMaxWidth().clip(RoundedCornerShape(13.dp)))
    Text("Descarga el mapa para usarlo sin señal", style = MaterialTheme.typography.headlineMedium,
        modifier = Modifier.semantics { heading() })
    Text("En muchos tramos no hay cobertura. Con el mapa en tu celular todo sigue funcionando.",
        style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.onSurfaceVariant)
    ProgresoMapa(ui.descarga)
    FilaAjuste("Usar datos móviles", detalle = if (ui.usarDatosMoviles) "Encendido: también sin Wi-Fi" else "Apagado: solo con Wi-Fi",
        alPulsar = { alCambiarMoviles(!ui.usarDatosMoviles) }) {
        Switch(checked = ui.usarDatosMoviles, onCheckedChange = alCambiarMoviles)
    }
}

/** «Corredor PE-28A · 335 km · 2,3 MB», el porcentaje, la barra y lo que pasa con palabras. */
@Composable
private fun ProgresoMapa(estado: EstadoDescarga) = Column(verticalArrangement = Arrangement.spacedBy(5.dp)) {
    val porcentaje = TextosDescarga.porcentaje(estado)
    val peso = TextosDescarga.peso(TextosDescarga.bytes(estado)).let { if (it.isEmpty()) "" else " · $it" }
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text("Corredor PE-28A · $LONGITUD_KM km$peso", fontWeight = FontWeight.Bold)
        Text("$porcentaje %", fontWeight = FontWeight.Bold)
    }
    LinearProgressIndicator(progress = { porcentaje / 100f }, modifier = Modifier.fillMaxWidth())
    Text(TextosDescarga.nota(estado), style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
}

/** Une [MapaSinConexionPantalla] con su ViewModel (pide la descarga al abrirse). */
@Composable
internal fun MapaSinConexionRuta(alSeguir: () -> Unit, alVolver: () -> Unit, vm: DescargaViewModel = hiltViewModel()) {
    val ui by vm.estado.collectAsState()
    MapaSinConexionPantalla(ui, vm::cambiarDatosMoviles, alSeguir, alVolver)
}
