package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.comun.ui.FilaCasilla
import pe.killauru.app.comun.ui.MarcoFormulario

/**
 * FR-001, FR-003, HU-27 (prototipo «Términos y privacidad»): los dos documentos en una sola pantalla; «Acepto y
 * continúo» se activa solo con las dos casillas marcadas. Avisa que, si la versión cambia, se volverá a pedir (R-024).
 */
@Composable
internal fun TerminosPantalla(terminosTexto: String, politicaTexto: String, version: String, onAceptar: () -> Unit) {
    var terminos by rememberSaveable { mutableStateOf(false) }
    var privacidad by rememberSaveable { mutableStateOf(false) }
    MarcoFormulario(desplazable = true) {
        Documento("Términos de uso", terminosTexto)
        Documento("Aviso de privacidad", politicaTexto)
        Casilla(terminos, { terminos = it }, "Acepto los términos de uso")
        Casilla(privacidad, { privacidad = it }, "Acepto el aviso de privacidad")
        Text(
            "Versión $version. Si cambia, te la mostraremos otra vez. Para solo ver el mapa sin enviar datos, " +
                "actívalo luego en Ajustes.",
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
        )
        Spacer(Modifier.height(21.dp))
        Button(onClick = onAceptar, enabled = terminos && privacidad, modifier = Modifier.fillMaxWidth()) {
            Text("Acepto y continúo")
        }
    }
}

@Composable
private fun Documento(titulo: String, texto: String) {
    Text(titulo, style = MaterialTheme.typography.headlineSmall)
    Spacer(Modifier.height(8.dp))
    Text(texto, style = MaterialTheme.typography.bodyMedium)
    Spacer(Modifier.height(21.dp))
}

@Composable
private fun Casilla(marcada: Boolean, alCambiar: (Boolean) -> Unit, texto: String) = FilaCasilla(marcada, alCambiar) {
    Spacer(Modifier.width(8.dp))
    Text(texto, style = MaterialTheme.typography.bodyLarge)
}
