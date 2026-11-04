package pe.killauru.app.mapa.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** FR-049: pantalla de detalle de una anomalía seleccionada en el mapa. */
@Composable
internal fun DetallePantalla(
    anomaliaId: String,
    onVolver: () -> Unit = {},
) {
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            Text("Detalle de anomalía", style = MaterialTheme.typography.titleLarge)
            Spacer(Modifier.height(8.dp))
            Text("ID: $anomaliaId")
            Spacer(Modifier.height(16.dp))
            OutlinedButton(onClick = onVolver) { Text("Volver") }
        }
    }
}
