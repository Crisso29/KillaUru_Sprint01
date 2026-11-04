package pe.killauru.app.mapa.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.mapa.dominio.AnomaliaLocal

/** FR-040, FR-049: pantalla de mapa con anomalías superpuestas. */
@Composable
internal fun MapaPantalla(
    viewModel: MapaViewModel = hiltViewModel(),
    onDetalle: (String) -> Unit = {},
) {
    val anomalias by viewModel.anomalias.collectAsState()
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Text("Mapa KillaUru", style = MaterialTheme.typography.titleLarge)
            Spacer(Modifier.height(8.dp))
            Text("${anomalias.size} anomalías visibles")
            Spacer(Modifier.height(8.dp))
            anomalias.take(5).forEach { anomalia ->
                TextButton(onClick = { onDetalle(anomalia.id) }) {
                    Text("${anomalia.tipo.texto} · km ${anomalia.km}")
                }
            }
        }
    }
}
