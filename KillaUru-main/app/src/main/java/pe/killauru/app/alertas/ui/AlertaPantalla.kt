package pe.killauru.app.alertas.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.alertas.dominio.Alerta

/** FR-054, FR-057: pantalla de alerta de proximidad a anomalía. */
@Composable
internal fun AlertaPantalla(
    alerta: Alerta? = null,
    onDismiss: () -> Unit = {},
) {
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            if (alerta != null) {
                Text("⚠ Anomalía próxima", style = MaterialTheme.typography.titleLarge)
                Spacer(Modifier.height(8.dp))
                Text("${alerta.anomalia.tipo.texto} a ${alerta.distanciaM.toInt()} metros")
                Text("Severidad: ${alerta.anomalia.severidad}")
                Spacer(Modifier.height(16.dp))
                Button(onClick = onDismiss) { Text("Entendido") }
            } else {
                Text("Sin alertas activas")
            }
        }
    }
}
