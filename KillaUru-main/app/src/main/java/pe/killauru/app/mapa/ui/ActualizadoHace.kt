package pe.killauru.app.mapa.ui

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier

/**
 * FR-052, R-034: chip que muestra cuánto tiempo ha pasado desde la última actualización.
 * Formato: «Actualizado hace X min» / «Actualizado hace X h» / «Ahora».
 */
@Composable
internal fun ActualizadoHace(
    ahoraMs: Long,
    verificadoUtc: Long?,
    modifier: Modifier = Modifier,
) {
    val texto = when {
        verificadoUtc == null -> "Sin actualizar"
        else -> {
            val deltaSeg = (ahoraMs - verificadoUtc) / 1_000L
            when {
                deltaSeg < 60 -> "Ahora"
                deltaSeg < 3_600 -> "Actualizado hace ${deltaSeg / 60} min"
                else -> "Actualizado hace ${deltaSeg / 3_600} h"
            }
        }
    }
    Text(
        text = texto,
        style = MaterialTheme.typography.labelSmall,
        modifier = modifier,
    )
}
