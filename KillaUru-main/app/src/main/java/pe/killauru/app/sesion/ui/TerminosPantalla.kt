package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.FilaCasilla
import pe.killauru.app.comun.ui.IconosKillaUru
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.Paso
import pe.killauru.app.comun.ui.textoConNegritas

/**
 * FR-001, FR-003, HU-27 (prototipo «Antes de empezar», paso 1 de 5): términos y aviso de privacidad en lenguaje simple
 * y en una sola pantalla; «Acepto y continúo» se activa solo con las dos casillas. Si la versión cambia, se pide otra vez.
 */
@Composable
internal fun TerminosPantalla(
    terminosTexto: String,
    politicaTexto: String,
    version: String,
    onAceptar: () -> Unit,
    alVolver: (() -> Unit)? = null,
) {
    var terminos by rememberSaveable { mutableStateOf(false) }
    var privacidad by rememberSaveable { mutableStateOf(false) }
    PantallaApp(
        titulo = "Antes de empezar",
        alVolver = alVolver,
        paso = Paso(1, PASOS_PRIMER_USO),
        pie = { BotonPrincipal("Acepto y continúo", onAceptar, habilitado = terminos && privacidad) },
    ) {
        Bloque("info", "Términos de uso", terminosTexto)
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
        Bloque("shield", "Aviso de privacidad", politicaTexto)
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
        Casilla(terminos, { terminos = it }, "Acepto los términos de uso")
        Casilla(privacidad, { privacidad = it }, "Acepto el aviso de privacidad")
        NotaVersion(version)
    }
}

/** FR-007: qué versión se acepta y que se volverá a pedir si cambia. */
@Composable
private fun NotaVersion(version: String) = Text(
    "Versión $version. Si cambia, te la mostraremos otra vez. Para solo ver el mapa sin enviar datos, actívalo luego en Ajustes.",
    style = MaterialTheme.typography.bodySmall,
    color = MaterialTheme.colorScheme.onSurfaceVariant,
)

/** Bloque del prototipo (`.blk`): título con ícono y una viñeta por línea del texto legal. */
@Composable
private fun Bloque(icono: String, titulo: String, texto: String) {
    Column(verticalArrangement = Arrangement.spacedBy(5.dp)) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Icon(IconosKillaUru.de(icono), contentDescription = null, modifier = Modifier.size(20.dp))
            Spacer(Modifier.width(8.dp))
            Text(titulo, fontSize = 15.sp, fontWeight = FontWeight.ExtraBold)
        }
        texto.lines().filter { it.isNotBlank() }.forEach { linea ->
            Row {
                Text("•  ", color = MaterialTheme.colorScheme.onSurfaceVariant)
                Text(textoConNegritas(linea), fontSize = 15.sp, color = MaterialTheme.colorScheme.onSurfaceVariant)
            }
        }
    }
}

@Composable
private fun Casilla(marcada: Boolean, alCambiar: (Boolean) -> Unit, texto: String) = FilaCasilla(marcada, alCambiar) {
    Spacer(Modifier.width(8.dp))
    Text(texto, fontSize = 15.sp)
}
