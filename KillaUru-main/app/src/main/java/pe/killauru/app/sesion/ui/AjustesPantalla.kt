package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** FR-005, FR-007, FR-008: pantalla de ajustes del usuario. */
@Composable
internal fun AjustesPantalla(
    onVerTerminos: () -> Unit = {},
    onVerPerfil: () -> Unit = {},
    viewModel: AjustesViewModel = hiltViewModel(),
) {
    val estado by viewModel.estado.collectAsState()

    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
        ) {
            Text("Ajustes", style = MaterialTheme.typography.headlineMedium)
            Spacer(Modifier.height(16.dp))

            Text("Vehículo", style = MaterialTheme.typography.labelLarge)
            CategoriaVehiculo.entries.forEach { cat ->
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    RadioButton(
                        selected = estado.categoria == cat,
                        onClick = { viewModel.cambiarCategoria(cat) },
                    )
                    Text(cat.texto)
                }
            }

            HorizontalDivider(modifier = Modifier.padding(vertical = 12.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically,
            ) {
                Column {
                    Text("Solo ver el mapa", style = MaterialTheme.typography.labelLarge)
                    Text(
                        "No detecta anomalías, solo muestra el mapa.",
                        style = MaterialTheme.typography.bodySmall,
                    )
                }
                Switch(
                    checked = estado.soloVerMapa,
                    onCheckedChange = { viewModel.cambiarSoloVerMapa(it) },
                )
            }

            Spacer(Modifier.height(8.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically,
            ) {
                Text("Usar datos móviles", style = MaterialTheme.typography.labelLarge)
                Switch(
                    checked = estado.usarDatosMoviles,
                    onCheckedChange = { viewModel.cambiarUsarDatosMoviles(it) },
                )
            }

            HorizontalDivider(modifier = Modifier.padding(vertical = 12.dp))

            estado.mapaVersion?.let {
                Text("Mapa: versión $it", style = MaterialTheme.typography.bodySmall)
            }
            estado.modeloVersion?.let {
                Text("Modelo: versión $it", style = MaterialTheme.typography.bodySmall)
            }

            Spacer(Modifier.height(16.dp))
            TextButton(onClick = onVerTerminos) { Text("Términos y condiciones") }
            TextButton(onClick = onVerPerfil) { Text("Mi perfil") }
        }
    }
}
