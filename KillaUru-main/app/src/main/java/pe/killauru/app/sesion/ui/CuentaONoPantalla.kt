package pe.killauru.app.sesion.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.IntrinsicSize
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.IconosKillaUru
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.Paso
import pe.killauru.app.comun.ui.tema.KillaUruTema

/**
 * FR-001, HU-21 (prototipo «¿Cuenta o sin cuenta?», paso 5 de 5): compara qué tienes sin cuenta y con cuenta, sin
 * presionar; «Ya tengo cuenta» aparece cuando la navegación ofrece el inicio de sesión.
 */
@Composable
internal fun CuentaONoPantalla(
    onCrearCuenta: () -> Unit,
    onContinuarSinCuenta: () -> Unit,
    onYaTengoCuenta: (() -> Unit)? = null,
    alVolver: (() -> Unit)? = null,
) {
    PantallaApp(
        titulo = "Último paso",
        alVolver = alVolver,
        paso = Paso(PASOS_PRIMER_USO, PASOS_PRIMER_USO),
        pie = { PieCuenta(onCrearCuenta, onContinuarSinCuenta, onYaTengoCuenta) },
    ) {
        Text("¿Creas una cuenta gratis?", style = MaterialTheme.typography.headlineMedium, modifier = Modifier.semantics { heading() })
        Text("Puedes usar KillaUru sin cuenta. Con cuenta ves toda la vía y te avisamos antes de cada bache.",
            style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Comparacion()
    }
}

/** «Crear cuenta gratis» destacado; «Seguir sin cuenta» y «Ya tengo cuenta» como enlaces (prototipo). */
@Composable
private fun PieCuenta(onCrearCuenta: () -> Unit, onContinuarSinCuenta: () -> Unit, onYaTengoCuenta: (() -> Unit)?) {
    BotonPrincipal("Crear cuenta gratis", onCrearCuenta)
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        TextButton(onClick = onContinuarSinCuenta) { Text("Seguir sin cuenta", fontWeight = FontWeight.Bold) }
        if (onYaTengoCuenta != null) {
            TextButton(onClick = onYaTengoCuenta) { Text("Ya tengo cuenta", fontWeight = FontWeight.Bold) }
        }
    }
}

private val SIN_CUENTA = listOf("Ayacucho – Vinchos (65 km)" to true, "Ayudas a detectar" to true, "Sin alertas" to false, "Sin historial" to false)
private val CON_CUENTA = listOf("Los 333 km", "Alerta antes de cada bache", "Tus viajes, solo en tu celular", "Gratis")

/** Prototipo `.cmp`: dos columnas, la de la cuenta resaltada; lo que no tienes va con ✕ y atenuado (no solo color). */
@Composable
private fun Comparacion() {
    val paleta = KillaUruTema.paleta
    val forma = RoundedCornerShape(13.dp)
    Row(Modifier.fillMaxWidth().height(IntrinsicSize.Min).clip(forma).border(1.dp, paleta.linea, forma)) {
        Columna("Sin cuenta", SIN_CUENTA, Modifier.weight(1f), Color.Transparent)
        Columna("Con cuenta", CON_CUENTA.map { it to true }, Modifier.weight(1f), paleta.killaSuave)
    }
}

@Composable
private fun Columna(titulo: String, items: List<Pair<String, Boolean>>, modifier: Modifier, fondo: Color) {
    val paleta = KillaUruTema.paleta
    Column(modifier.fillMaxHeight().background(fondo).padding(13.dp), verticalArrangement = Arrangement.spacedBy(5.dp)) {
        Text(titulo, fontSize = 16.sp, fontWeight = FontWeight.ExtraBold)
        items.forEach { (texto, incluido) ->
            Row(verticalAlignment = Alignment.Top) {
                Icon(IconosKillaUru.de(if (incluido) "check" else "close"), contentDescription = if (incluido) "Incluye" else "No incluye",
                    modifier = Modifier.size(16.dp).padding(top = 2.dp), tint = if (incluido) paleta.tinta else paleta.tinta3)
                Text(" $texto", fontSize = 14.sp, color = if (incluido) paleta.tinta else paleta.tinta3)
            }
        }
    }
}
