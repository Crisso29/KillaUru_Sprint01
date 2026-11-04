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
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@Composable
internal fun PerfilPantalla(
    onCerrarSesion: () -> Unit,
    onEliminarCuenta: () -> Unit,
    viewModel: PerfilViewModel = hiltViewModel(),
) {
    val estado by viewModel.estado.collectAsState()

    Scaffold(topBar = { TopAppBar(title = { Text("Mi perfil") }) }) { padding ->
        Column(Modifier.padding(padding).padding(16.dp)) {
            if (estado.perfil != null) {
                Text("Correo: ${estado.perfil!!.correo}")
                Text("Rol: ${estado.perfil!!.rol}")
                if (!estado.perfil!!.verificado) Text("Cuenta sin verificar.")
            }
            Spacer(Modifier.height(16.dp))
            OutlinedButton(onClick = onCerrarSesion, modifier = Modifier.fillMaxWidth()) {
                Text("Cerrar sesión")
            }
            Spacer(Modifier.height(8.dp))
            Button(onClick = onEliminarCuenta, modifier = Modifier.fillMaxWidth()) {
                Text("Eliminar cuenta")
            }
        }
    }
}
