package pe.killauru.app.comun.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

/** Lista con viñetas del prototipo (`ul`): admite `**negrita**` en cada línea. */
@Composable
internal fun Vinetas(lineas: List<String>) = Column(verticalArrangement = Arrangement.spacedBy(5.dp)) {
    lineas.forEach { linea ->
        Row {
            Text("•  ", color = MaterialTheme.colorScheme.onSurfaceVariant)
            Text(textoConNegritas(linea), fontSize = 15.sp, color = MaterialTheme.colorScheme.onSurfaceVariant)
        }
    }
}
