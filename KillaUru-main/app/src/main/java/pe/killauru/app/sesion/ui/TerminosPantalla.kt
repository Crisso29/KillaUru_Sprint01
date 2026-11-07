package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** FR-001, FR-003, FR-008: pantalla de términos y política con doble casilla obligatoria. */
@Composable
internal fun TerminosPantalla(
    terminosTexto: String,
    politicaTexto: String,
    onAceptar: () -> Unit,
) {
    var terminosMarcado by remember { mutableStateOf(false) }
    var politicaMarcada by remember { mutableStateOf(false) }
    val puedeAceptar = terminosMarcado && politicaMarcada

    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp)
                .verticalScroll(rememberScrollState()),
        ) {
            Text("Términos y condiciones", style = MaterialTheme.typography.headlineSmall)
            Spacer(Modifier.height(8.dp))
            Text(terminosTexto, style = MaterialTheme.typography.bodyMedium)
            Spacer(Modifier.height(16.dp))
            Text("Política de privacidad", style = MaterialTheme.typography.headlineSmall)
            Spacer(Modifier.height(8.dp))
            Text(politicaTexto, style = MaterialTheme.typography.bodyMedium)
            Spacer(Modifier.height(24.dp))
            Row(verticalAlignment = androidx.compose.ui.Alignment.CenterVertically) {
                Checkbox(checked = terminosMarcado, onCheckedChange = { terminosMarcado = it })
                Spacer(Modifier.width(8.dp))
                Text("Acepto los términos y condiciones")
            }
            Row(verticalAlignment = androidx.compose.ui.Alignment.CenterVertically) {
                Checkbox(checked = politicaMarcada, onCheckedChange = { politicaMarcada = it })
                Spacer(Modifier.width(8.dp))
                Text("Acepto la política de privacidad")
            }
            Spacer(Modifier.height(24.dp))
            Button(
                onClick = onAceptar,
                enabled = puedeAceptar,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text("Continuar")
            }
        }
    }
}
