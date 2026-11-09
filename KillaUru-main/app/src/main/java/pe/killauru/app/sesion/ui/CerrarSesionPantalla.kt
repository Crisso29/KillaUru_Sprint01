@file:OptIn(androidx.compose.material3.ExperimentalMaterial3Api::class)

package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@Composable
internal fun CerrarSesionPantalla(
    pendientes: Int,
    onConfirmar: () -> Unit,
    onCancelar: () -> Unit,
    viewModel: CerrarSesionViewModel = hiltViewModel(),
) {
    Scaffold(topBar = { TopAppBar(title = { Text("Cerrar sesión") }) }) { padding ->
        Column(Modifier.padding(padding).padding(16.dp)) {
            if (pendientes > 0) {
                Text("Tienes $pendientes eventos pendientes de enviar.")
                Text("Al cerrar sesión se enviarán sin cuenta.")
            }
            Spacer(Modifier.height(16.dp))
            Button(
                onClick = { viewModel.cerrarSesion(onConfirmar) },
                modifier = Modifier.fillMaxWidth(),
            ) { Text("Cerrar sesión") }
            Spacer(Modifier.height(8.dp))
            OutlinedButton(onClick = onCancelar, modifier = Modifier.fillMaxWidth()) {
                Text("Cancelar")
            }
        }
    }
}
