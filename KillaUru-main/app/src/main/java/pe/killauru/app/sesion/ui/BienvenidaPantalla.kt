package pe.killauru.app.sesion.ui

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import pe.killauru.app.R
import androidx.compose.foundation.layout.ColumnScope
import pe.killauru.app.comun.ui.BotonPrincipal

// Colores fijos de la ilustración nocturna del prototipo (cielo #0B2433 → #11475A): la escena es de noche también en
// modo claro, como en el prototipo y el logo; por eso no salen de la paleta clara/oscura.
private val CIELO = Brush.verticalGradient(listOf(Color(0xFF0B2433), Color(0xFF11475A)))
private val SUELO = Color(0xFF08141A)
private val TEXTO = Color(0xFFE6F0F2)
private val TEXTO_SUAVE = Color(0xFFA6BAC0)

/**
 * FR-001 (prototipo «Bienvenida»): la luciérnaga-marcador alumbra la vía en el 61,8 % superior; el mensaje y un único
 * botón viven en el 38,2 % inferior. Sin registro obligatorio aquí.
 */
@Composable
internal fun BienvenidaPantalla(onComenzar: () -> Unit) {
    Column(Modifier.fillMaxSize().background(SUELO)) {
        Portada()
        Lema(onComenzar)
    }
}

/** Lema, explicación y «Empezar» en la parte de abajo (0,382). */
@Composable
private fun ColumnScope.Lema(onComenzar: () -> Unit) {
    Column(
        modifier = Modifier.weight(0.382f).fillMaxWidth().padding(horizontal = 21.dp, vertical = 21.dp),
        verticalArrangement = Arrangement.spacedBy(13.dp),
    ) {
        Text("Baches avisados a tiempo.", style = MaterialTheme.typography.displayMedium, color = TEXTO,
            modifier = Modifier.semantics { heading() })
        Text("Tu celular los detecta solo mientras manejas por la Vía Los Libertadores. Entre todos armamos el mapa.",
            fontSize = 16.sp, fontWeight = FontWeight.Normal, color = TEXTO_SUAVE)
        Box(Modifier.weight(1f))
        BotonPrincipal("Empezar", onComenzar, luna = true)
    }
}

/** Cielo nocturno con el logo en la proporción áurea de arriba (0,618), como el prototipo. */
@Composable
private fun ColumnScope.Portada() {
    Box(Modifier.weight(0.618f).fillMaxWidth().background(CIELO), contentAlignment = Alignment.Center) {
        Image(
            painter = painterResource(R.drawable.logo_senal),
            contentDescription = "KillaUru: una luciérnaga con forma de marcador alumbra la vía entre montañas",
            modifier = Modifier.size(233.dp),
        )
    }
}
