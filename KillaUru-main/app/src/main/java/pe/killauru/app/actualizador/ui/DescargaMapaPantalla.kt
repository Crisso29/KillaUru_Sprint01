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
// TopAppBar sigue marcada como experimental en Material 3: su uso debe declararse explícitamente.
@OptIn(ExperimentalMaterial3Api::class)
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
            ContenidoDescarga(estado, alSalir)
        }
    }
}

@Composable
private fun ContenidoDescarga(estado: DescargaViewModel.Estado, alSalir: () -> Unit) = when (estado) {
    is DescargaViewModel.Estado.Esperando -> Esperando()
    is DescargaViewModel.Estado.Descargando -> Descargando(estado.progreso)
    is DescargaViewModel.Estado.Completado -> Completado(alSalir)
    is DescargaViewModel.Estado.Fallido -> Text(stringResource(R.string.descarga_mapa_error), color = MaterialTheme.colorScheme.error)
}

@Composable
private fun Esperando() {
    CircularProgressIndicator()
    Spacer(Modifier.height(16.dp))
    Text(stringResource(R.string.descarga_mapa_esperando))
}

/** Barra y porcentaje; avisa que se puede cerrar la app y la descarga sigue (FR-044, reanudable). */
@Composable
private fun Descargando(progreso: Int) {
    LinearProgressIndicator(progress = { progreso / 100f }, modifier = Modifier.fillMaxWidth())
    Spacer(Modifier.height(8.dp))
    Text("$progreso %")
    Spacer(Modifier.height(8.dp))
    Text(
        text = stringResource(R.string.descarga_mapa_reanudable),
        style = MaterialTheme.typography.bodySmall,
        color = MaterialTheme.colorScheme.onSurfaceVariant,
    )
}

@Composable
private fun Completado(alSalir: () -> Unit) {
    Text(stringResource(R.string.descarga_mapa_listo))
    Spacer(Modifier.height(16.dp))
    Button(onClick = alSalir) { Text(stringResource(R.string.descarga_mapa_abrir)) }
}
