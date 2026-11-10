package pe.killauru.app.actualizador.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import pe.killauru.app.R

/**
 * FR-041, FR-008: pantalla que muestra el progreso de la descarga del mapa PMTiles.
 * Texto «Si se corta, sigue desde donde quedó» para explicar la descarga reanudable.
 */
@Composable
internal fun DescargaMapaPantalla(
    alSalir: () -> Unit,
    viewModel: DescargaViewModel = hiltViewModel(),
) {
    val estado by viewModel.estado.collectAsStateWithLifecycle()

    Scaffold(
        topBar = {
            TopAppBar(title = { Text(stringResource(R.string.descarga_mapa_titulo)) })
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .padding(padding)
                .fillMaxSize()
                .padding(24.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            when (val s = estado) {
                is DescargaViewModel.Estado.Esperando -> {
                    CircularProgressIndicator()
                    Spacer(Modifier.height(16.dp))
                    Text(stringResource(R.string.descarga_mapa_esperando))
                }
                is DescargaViewModel.Estado.Descargando -> {
                    LinearProgressIndicator(
                        progress = { s.progreso / 100f },
                        modifier = Modifier.fillMaxWidth(),
                    )
                    Spacer(Modifier.height(8.dp))
                    Text("${s.progreso} %")
                    Spacer(Modifier.height(8.dp))
                    // Explica al usuario que puede cerrar la app y la descarga sigue
                    Text(
                        text = stringResource(R.string.descarga_mapa_reanudable),
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }
                is DescargaViewModel.Estado.Completado -> {
                    Text(stringResource(R.string.descarga_mapa_listo))
                    Spacer(Modifier.height(16.dp))
                    Button(onClick = alSalir) {
                        Text(stringResource(R.string.descarga_mapa_abrir))
                    }
                }
                is DescargaViewModel.Estado.Fallido -> {
                    Text(
                        text = stringResource(R.string.descarga_mapa_error),
                        color = MaterialTheme.colorScheme.error,
                    )
                }
            }
        }
    }
}
