package pe.killauru.app.comun.ui

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

/** Encabezado de grupo en Ajustes: «DETECCIÓN», «MAPA SIN CONEXIÓN»… (prototipo `.set-h`). */
@Composable
internal fun TituloSeccion(texto: String) {
    Text(
        texto.uppercase(),
        style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.ExtraBold, letterSpacing = 1.sp),
        color = MaterialTheme.colorScheme.onSurfaceVariant,
        modifier = Modifier.padding(top = 21.dp).semantics { heading() },
    )
}

/**
 * Fila de Ajustes (prototipo `.set`): título, explicación y a la derecha un valor, un interruptor o «›». Si tiene
 * `alPulsar`, toda la fila es un botón; la línea inferior separa las filas como en el prototipo.
 */
@Composable
internal fun FilaAjuste(
    titulo: String,
    modifier: Modifier = Modifier,
    detalle: String? = null,
    alPulsar: (() -> Unit)? = null,
    final: @Composable RowScope.() -> Unit = { if (alPulsar != null) Chevron() },
) {
    Column(modifier) {
        Row(
            modifier = Modifier.fillMaxWidth().heightIn(min = 55.dp)
                .then(if (alPulsar != null) Modifier.clickable(onClick = alPulsar) else Modifier)
                .padding(vertical = 13.dp),
            horizontalArrangement = Arrangement.spacedBy(13.dp),
            verticalAlignment = Alignment.CenterVertically,
        ) {
            Column(Modifier.weight(1f)) {
                Text(titulo, fontSize = 16.sp, fontWeight = FontWeight.SemiBold)
                if (detalle != null) {
                    Text(detalle, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                }
            }
            final()
        }
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
    }
}

/** «›» al final de una fila que abre otra pantalla. */
@Composable
internal fun Chevron() = Icon(IconosKillaUru.de("chev"), contentDescription = null, modifier = Modifier.size(18.dp),
    tint = MaterialTheme.colorScheme.onSurfaceVariant)
