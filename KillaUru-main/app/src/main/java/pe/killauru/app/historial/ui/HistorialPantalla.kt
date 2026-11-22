package pe.killauru.app.historial.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.IntrinsicSize
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.withStyle
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.TonoAviso
import pe.killauru.app.comun.ui.tema.KillaUruTema
import pe.killauru.app.historial.dominio.ViajeRecorrido
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.util.Locale
import kotlin.math.roundToInt

private val LIMA = ZoneId.of("America/Lima")
private val MES = DateTimeFormatter.ofPattern("MMM", Locale.forLanguageTag("es-PE"))

/** HU-10, RF-25 (prototipo «Mis viajes»): tus viajes y lo que detectaste; vive solo en tu celular 6 meses (RN-21). */
@Composable
internal fun HistorialPantalla(alCrearCuenta: () -> Unit = {}, viewModel: HistorialViewModel = hiltViewModel()) {
    val state by viewModel.uiState.collectAsState()
    Surface(Modifier.fillMaxSize()) {
        LazyColumn(Modifier.fillMaxSize().padding(horizontal = 21.dp), verticalArrangement = Arrangement.spacedBy(13.dp)) {
            item {
                Text("Mis viajes", style = MaterialTheme.typography.headlineMedium,
                    modifier = Modifier.padding(top = 13.dp).semantics { heading() })
            }
            item { AvisoPrivacidad() }
            viajes(state, alCrearCuenta)
        }
    }
}

/** Sin cuenta, invita a crearla; con cuenta y sin viajes, lo explica; si hay viajes, el resumen y la lista. */
private fun LazyListScope.viajes(state: HistorialUiState, alCrearCuenta: () -> Unit) = when {
    !state.registrado -> item {
        Column(verticalArrangement = Arrangement.spacedBy(13.dp)) {
            Vacio("Con una cuenta, cada viaje queda aquí con los baches que detectaste.")
            BotonPrincipal("Crear cuenta gratis", alCrearCuenta)
        }
    }
    state.recorridos.isEmpty() -> item { Vacio("Aún no hay viajes. Aparecen al terminar tu primer recorrido.") }
    else -> {
        item { Resumen(state.recorridos) }
        items(state.recorridos, key = { it.id }) { FilaViaje(it) }
    }
}

@Composable
private fun AvisoPrivacidad() = Aviso("phone", TonoAviso.KILLA) {
    Text(buildAnnotatedString {
        append("Se guarda ")
        withStyle(SpanStyle(fontWeight = FontWeight.Bold)) { append("solo en este celular") }
        append(" por 6 meses. Ni nosotros lo vemos. Se borra si cierras sesión.")
    }, style = MaterialTheme.typography.bodyMedium)
}

@Composable
private fun Vacio(texto: String) =
    Text(texto, style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.onSurfaceVariant)

/** Dos cifras grandes del prototipo: km de los últimos 6 meses y baches detectados. */
@Composable
private fun Resumen(viajes: List<ViajeRecorrido>) {
    val paleta = KillaUruTema.paleta
    val forma = RoundedCornerShape(13.dp)
    Row(Modifier.fillMaxWidth().height(IntrinsicSize.Min).border(1.dp, paleta.linea, forma)) {
        Cifra("Últimos 6 meses", "${viajes.sumOf { it.kmRecorridos }.roundToInt()} km", Modifier.weight(1f))
        Column(Modifier.width(1.dp).fillMaxHeight().background(paleta.linea)) {}
        Cifra("Baches detectados", "${viajes.sumOf { it.eventos }}", Modifier.weight(1f))
    }
}

@Composable
private fun Cifra(etiqueta: String, valor: String, modifier: Modifier) = Column(modifier.padding(13.dp)) {
    Text(etiqueta, fontSize = 13.sp, color = MaterialTheme.colorScheme.onSurfaceVariant)
    Text(valor, style = MaterialTheme.typography.headlineMedium)
}

/** Fila del prototipo: fecha grande, tramo por pueblos, km y duración, y la insignia de baches. */
@Composable
private fun FilaViaje(v: ViajeRecorrido) = Column {
    Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(13.dp)) {
        FechaViaje(v.inicioMs)
        Column(Modifier.weight(1f)) {
            Text("${TextosViaje.lugar(v.kmDesde)} → ${TextosViaje.lugar(v.kmHasta)}", fontWeight = FontWeight.Bold)
            Text(TextosViaje.detalle(v.kmDesde, v.kmHasta, v.inicioMs, v.finMs), fontSize = 13.sp,
                color = MaterialTheme.colorScheme.onSurfaceVariant)
        }
        InsigniaBaches(v.eventos)
    }
    HorizontalDivider(Modifier.padding(top = 13.dp), color = MaterialTheme.colorScheme.outlineVariant)
}

/** Día grande y mes abreviado («08 / NOV»), en hora de Lima. */
@Composable
private fun FechaViaje(inicioMs: Long) {
    val inicio = Instant.ofEpochMilli(inicioMs).atZone(LIMA)
    Column(Modifier.width(34.dp), horizontalAlignment = Alignment.CenterHorizontally) {
        Text("%02d".format(inicio.dayOfMonth), style = MaterialTheme.typography.titleLarge)
        Text(MES.format(inicio).replace(".", "").uppercase(), fontSize = 11.sp, fontWeight = FontWeight.Bold,
            color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

/** Cuántos baches detectó en ese viaje, en el dorado de aviso del prototipo. */
@Composable
private fun InsigniaBaches(eventos: Int) {
    val paleta = KillaUruTema.paleta
    Column(Modifier.background(paleta.lunaSuave, RoundedCornerShape(6.dp)).padding(horizontal = 8.dp, vertical = 2.dp),
        horizontalAlignment = Alignment.CenterHorizontally) {
        Text("$eventos", style = MaterialTheme.typography.titleLarge, color = paleta.aviso, textAlign = TextAlign.Center)
        Text("baches", fontSize = 11.sp, color = paleta.aviso)
    }
}
