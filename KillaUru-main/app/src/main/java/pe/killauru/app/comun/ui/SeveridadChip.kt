package pe.killauru.app.comun.ui

import androidx.compose.material3.AssistChip
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

/** FR-008: chip de severidad reutilizable en Mapa y Detalle. */
@Composable
internal fun SeveridadChip(severidad: Int) {
    val color = when (severidad) {
        1, 2 -> Color(0xFF4CAF50)
        3 -> Color(0xFFFFC107)
        4, 5 -> Color(0xFFF44336)
        else -> Color.Gray
    }
    val etiqueta = "Severidad $severidad"
    AssistChip(
        onClick = {},
        label = { Text(etiqueta, color = color) },
    )
}
