@file:OptIn(androidx.compose.material3.ExperimentalMaterial3Api::class)

package pe.killauru.app.historial.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@Composable
internal fun HistorialPantalla(viewModel: HistorialViewModel = hiltViewModel()) {
    val state by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = { TopAppBar(title = { Text("Mis viajes") }) }
    ) { padding ->
        ContenidoHistorial(state, padding)
    }
}

@Composable
private fun ContenidoHistorial(state: HistorialUiState, padding: androidx.compose.foundation.layout.PaddingValues) {
        if (!state.registrado) {
            Column(Modifier.padding(padding).padding(16.dp)) {
                Text("Inicia sesión para guardar tu historial de viajes.")
            }
        } else if (state.recorridos.isEmpty()) {
            Column(Modifier.padding(padding).padding(16.dp)) {
                Text("No tienes viajes registrados aún.")
                Text("Se guarda solo en este celular por 6 meses automáticamente.",
                    style = MaterialTheme.typography.bodySmall)
            }
        } else {
            Recorridos(state, padding)
        }
}

@Composable
private fun Recorridos(state: HistorialUiState, padding: androidx.compose.foundation.layout.PaddingValues) {
            LazyColumn(Modifier.fillMaxSize().padding(padding)) {
                items(state.recorridos) { r ->
                    Column(Modifier.padding(horizontal = 16.dp, vertical = 8.dp)) {
                        Text("${String.format(java.util.Locale.forLanguageTag("es-PE"), "%.1f", r.kmRecorridos)} km recorridos")
                        Text("${r.eventos} eventos detectados",
                            style = MaterialTheme.typography.bodySmall)
                    }
                }
            }
}
