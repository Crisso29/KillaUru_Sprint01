package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp

/** FR-065: pantalla de registro de nueva cuenta. */
@Composable
internal fun RegistroPantalla(
    onRegistrar: (correo: String, contrasena: String) -> Unit = { _, _ -> },
    onYaTengoCuenta: () -> Unit = {},
) {
    var correo by remember { mutableStateOf("") }
    var contrasena by remember { mutableStateOf("") }
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            Text("Crear cuenta", style = MaterialTheme.typography.titleLarge)
            Spacer(Modifier.height(16.dp))
            OutlinedTextField(
                value = correo, onValueChange = { correo = it },
                label = { Text("Correo electrónico") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
                modifier = Modifier.fillMaxWidth(),
            )
            Spacer(Modifier.height(8.dp))
            OutlinedTextField(
                value = contrasena, onValueChange = { contrasena = it },
                label = { Text("Contraseña") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth(),
            )
            Spacer(Modifier.height(16.dp))
            Button(
                onClick = { onRegistrar(correo, contrasena) },
                enabled = correo.isNotBlank() && contrasena.isNotBlank(),
                modifier = Modifier.fillMaxWidth(),
            ) { Text("Registrarme") }
            Spacer(Modifier.height(8.dp))
            TextButton(onClick = onYaTengoCuenta) { Text("Ya tengo cuenta") }
        }
    }
}
