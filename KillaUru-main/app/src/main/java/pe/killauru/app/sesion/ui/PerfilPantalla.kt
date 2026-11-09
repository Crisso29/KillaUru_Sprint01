package pe.killauru.app.sesion.ui

import pe.killauru.app.comun.ui.MarcoFormulario

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** FR-067, FR-069: pantalla de perfil con opción de cerrar sesión y eliminar cuenta. */
@Composable
internal fun PerfilPantalla(
    usuarioId: String = "",
    onCerrarSesion: () -> Unit = {},
    onEliminarCuenta: () -> Unit = {},
    onVolver: () -> Unit = {},
) {
    MarcoFormulario(alineado = true) {
            Spacer(Modifier.height(32.dp))
            Text("Mi perfil", style = MaterialTheme.typography.titleLarge)
            Spacer(Modifier.height(16.dp))
            if (usuarioId.isNotBlank()) {
                Text("ID: $usuarioId", style = MaterialTheme.typography.bodyMedium)
            }
            Spacer(Modifier.weight(1f))
            OutlinedButton(
                onClick = onCerrarSesion,
                modifier = Modifier.fillMaxWidth(),
            ) { Text("Cerrar sesión") }
            Spacer(Modifier.height(8.dp))
            TextButton(onClick = onEliminarCuenta) { Text("Eliminar cuenta") }
            Spacer(Modifier.height(8.dp))
            TextButton(onClick = onVolver) { Text("Volver") }
    }
}
