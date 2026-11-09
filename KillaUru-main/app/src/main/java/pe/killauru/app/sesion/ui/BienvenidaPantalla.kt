package pe.killauru.app.sesion.ui

import pe.killauru.app.comun.ui.MarcoFormulario

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** FR-001, FR-008: pantalla de bienvenida del primer uso. */
@Composable
internal fun BienvenidaPantalla(onComenzar: () -> Unit) {
    MarcoFormulario(centrado = true, alineado = true) {
            Text(
                text = "KillaUru",
                style = MaterialTheme.typography.displayMedium,
            )
            Spacer(Modifier.height(8.dp))
            Text(
                text = "Detecta baches y hundimientos en la vía.",
                style = MaterialTheme.typography.bodyLarge,
            )
            Spacer(Modifier.height(32.dp))
            Button(onClick = onComenzar, modifier = Modifier.fillMaxWidth()) {
                Text("Comenzar")
            }
    }
}
