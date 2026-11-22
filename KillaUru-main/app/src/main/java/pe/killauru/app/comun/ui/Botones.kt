package pe.killauru.app.comun.ui

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import pe.killauru.app.comun.ui.tema.KillaUruTema

private val FORMA = RoundedCornerShape(13.dp)
private const val ALTO_DP = 55 // prototipo `.btn-xl`: botones grandes, fáciles de tocar con el vehículo detenido

/** Botón principal a todo el ancho (prototipo `.btn-primary.btn-xl`). `luna = true`: el dorado de la bienvenida. */
@Composable
internal fun BotonPrincipal(
    texto: String,
    alPulsar: () -> Unit,
    modifier: Modifier = Modifier,
    habilitado: Boolean = true,
    luna: Boolean = false,
) {
    val paleta = KillaUruTema.paleta
    val colores = if (luna) {
        ButtonDefaults.buttonColors(containerColor = paleta.luna, contentColor = paleta.tinta)
    } else {
        ButtonDefaults.buttonColors()
    }
    Button(
        onClick = alPulsar,
        enabled = habilitado,
        shape = FORMA,
        colors = colores,
        modifier = modifier.fillMaxWidth().heightIn(min = ALTO_DP.dp),
    ) { Text(texto, fontWeight = FontWeight.Bold, fontSize = 16.sp) }
}

/** Botón secundario con borde (prototipo `.btn-ghost.btn-xl`). */
@Composable
internal fun BotonSecundario(texto: String, alPulsar: () -> Unit, modifier: Modifier = Modifier) {
    OutlinedButton(
        onClick = alPulsar,
        shape = FORMA,
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outline),
        modifier = modifier.fillMaxWidth().heightIn(min = ALTO_DP.dp),
    ) { Text(texto, fontWeight = FontWeight.Bold, fontSize = 16.sp, color = MaterialTheme.colorScheme.onSurface) }
}
