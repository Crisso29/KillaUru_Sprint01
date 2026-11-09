@file:OptIn(androidx.compose.material3.ExperimentalMaterial3Api::class)

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
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable
internal fun OlvideContrasenaPantalla(onEnviado: () -> Unit) {
    var correo by rememberSaveable { mutableStateOf("") }

    Scaffold(topBar = { TopAppBar(title = { Text("Recuperar contraseña") }) }) { padding ->
        Column(Modifier.padding(padding).padding(16.dp)) {
            Text("Ingresa tu correo y te enviaremos un enlace de recuperación.")
            Spacer(Modifier.height(8.dp))
            OutlinedTextField(correo, { correo = it }, label = { Text("Correo") },
                modifier = Modifier.fillMaxWidth())
            Spacer(Modifier.height(8.dp))
            Button(onClick = onEnviado, modifier = Modifier.fillMaxWidth()) {
                Text("Enviar enlace")
            }
        }
    }
}
