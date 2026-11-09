@file:OptIn(androidx.compose.material3.ExperimentalMaterial3Api::class)

package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable
internal fun VerificarCorreoPantalla(correo: String) {
    Scaffold(topBar = { TopAppBar(title = { Text("Revisa tu correo") }) }) { padding ->
        Column(Modifier.padding(padding).padding(16.dp)) {
            Text("Enviamos un correo a $correo.")
            Text("Haz clic en el enlace para activar tu cuenta.")
            Text("Puedes usar la app como anónimo mientras tanto.")
        }
    }
}
