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

@Composable
internal fun CamposCredenciales(correo: String, contrasena: String, onCorreo: (String) -> Unit,
    onContrasena: (String) -> Unit, etiquetaCorreo: String) {
            OutlinedTextField(
                value = correo, onValueChange = onCorreo,
                label = { Text(etiquetaCorreo) },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
                modifier = Modifier.fillMaxWidth(),
            )
            Spacer(Modifier.height(8.dp))
            OutlinedTextField(
                value = contrasena, onValueChange = onContrasena,
                label = { Text("Contraseña") },
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth(),
            )
}
