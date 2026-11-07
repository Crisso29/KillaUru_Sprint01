package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.Checkbox
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@Composable
internal fun EliminarCuentaPantalla(
    onEliminada: () -> Unit,
    viewModel: EliminarCuentaViewModel = hiltViewModel(),
) {
    var contrasena by rememberSaveable { mutableStateOf("") }
    var confirmado by rememberSaveable { mutableStateOf(false) }

    Scaffold(topBar = { TopAppBar(title = { Text("Eliminar cuenta") }) }) { padding ->
        Column(Modifier.padding(padding).padding(16.dp)) {
            Text("Esta acción es permanente. Se eliminarán todos tus datos del servidor.")
            Spacer(Modifier.height(8.dp))
            OutlinedTextField(contrasena, { contrasena = it },
                label = { Text("Contraseña") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth())
            Spacer(Modifier.height(8.dp))
            Row(verticalAlignment = Alignment.CenterVertically) {
                Checkbox(confirmado, { confirmado = it })
                Text("Confirmo que quiero eliminar mi cuenta.")
            }
            Spacer(Modifier.height(8.dp))
            Button(
                onClick = { viewModel.eliminar(contrasena, onEliminada) },
                enabled = confirmado && contrasena.isNotBlank(),
                modifier = Modifier.fillMaxWidth(),
            ) { Text("Eliminar definitivamente") }
        }
    }
}
