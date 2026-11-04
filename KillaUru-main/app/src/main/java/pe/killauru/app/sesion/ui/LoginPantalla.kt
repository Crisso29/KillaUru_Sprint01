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

/** FR-063: pantalla de inicio de sesión. */
@Composable
internal fun LoginPantalla(
    onLogin: (correo: String, contrasena: String) -> Unit = { _, _ -> },
    onCrearCuenta: () -> Unit = {},
    onOlvideContrasena: () -> Unit = {},
) {
    var correo by remember { mutableStateOf("") }
    var contrasena by remember { mutableStateOf("") }
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            Text("Iniciar sesión", style = MaterialTheme.typography.titleLarge)
            Spacer(Modifier.height(16.dp))
            OutlinedTextField(
                value = correo, onValueChange = { correo = it },
                label = { Text("Correo") },
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
                onClick = { onLogin(correo, contrasena) },
                enabled = correo.isNotBlank() && contrasena.isNotBlank(),
                modifier = Modifier.fillMaxWidth(),
            ) { Text("Ingresar") }
            TextButton(onClick = onOlvideContrasena) { Text("Olvidé mi contraseña") }
            TextButton(onClick = onCrearCuenta) { Text("Crear cuenta") }
        }
    }
}
