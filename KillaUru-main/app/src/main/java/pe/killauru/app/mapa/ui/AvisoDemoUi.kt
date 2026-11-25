package pe.killauru.app.mapa.ui

import androidx.compose.material3.Snackbar
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier

/** T129 / FR-049, FR-051, RN-08: banner «Estás fuera del tramo demo», una vez por viaje. */
@Composable
internal fun AvisoDemoUi(visible: Boolean, alDescartar: () -> Unit, modifier: Modifier = Modifier) {
    if (!visible) return
    Snackbar(
        modifier = modifier,
        action = { TextButton(onClick = alDescartar) { Text("OK") } },
    ) {
        Text("Estás fuera del tramo demo")
    }
}
