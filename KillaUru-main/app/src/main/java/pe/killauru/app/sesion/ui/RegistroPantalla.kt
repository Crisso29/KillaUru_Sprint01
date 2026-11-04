package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@Composable
internal fun RegistroPantalla(
    onRegistrado: () -> Unit,
    viewModel: RegistroViewModel = hiltViewModel(),
) {
    val estado by viewModel.estado.collectAsState()
    var correo by rememberSaveable { mutableStateOf("") }
    var contrasena by rememberSaveable { mutableStateOf("") }

    Scaffold(topBar = { TopAppBar(title = { Text("Crear cuenta") }) }) { padding ->
        Column(Modifier.padding(padding).padding(16.dp)) {
            OutlinedTextField(correo, { correo = it }, label = { Text("Correo") },
                modifier = Modifier.fillMaxWidth())
            Spacer(Modifier.height(8.dp))
            OutlinedTextField(contrasena, { contrasena = it }, label = { Text("Contraseña") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth())
            if (estado.error != null) { Spacer(Modifier.height(4.dp)); Text(estado.error!!) }
            Spacer(Modifier.height(8.dp))
            Button(onClick = { viewModel.registrar(correo, contrasena, onRegistrado) },
                modifier = Modifier.fillMaxWidth()) {
                Text("Crear cuenta")
            }
        }
    }
}
