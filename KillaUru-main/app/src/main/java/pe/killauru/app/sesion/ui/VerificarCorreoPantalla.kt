package pe.killauru.app.sesion.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.withStyle
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.IconosKillaUru
import pe.killauru.app.comun.ui.TonoAviso
import pe.killauru.app.comun.ui.tema.KillaUruTema

/**
 * FR-064, HU-18b (prototipo «Revisa tu correo»): la cuenta ya sirve; verificar el correo solo hace falta para poder
 * recuperar la contraseña. «Reenviar el correo» pide otro enlace (vence en 24 h).
 */
@Composable
internal fun VerificarCorreoPantalla(correo: String, reenviado: Boolean, alIrAlMapa: () -> Unit, alReenviar: () -> Unit) {
    Surface(Modifier.fillMaxSize()) {
        // Con scroll: con letra grande (WCAG 1.4.10) el botón no debe quedar fuera de la pantalla.
        Column(Modifier.verticalScroll(rememberScrollState()).padding(21.dp), verticalArrangement = Arrangement.spacedBy(13.dp, Alignment.CenterVertically),
            horizontalAlignment = Alignment.CenterHorizontally) {
            IconoCorreo()
            Text("Revisa tu correo", style = MaterialTheme.typography.headlineMedium, modifier = Modifier.semantics { heading() })
            Text(buildAnnotatedString {
                append("Enviamos un enlace a ")
                withStyle(SpanStyle(fontWeight = FontWeight.Bold)) { append(correo) }
                append(". Vence en 24 horas.")
            }, textAlign = TextAlign.Center, color = MaterialTheme.colorScheme.onSurfaceVariant)
            Aviso("info", TonoAviso.INFO, "Ya puedes usar los 333 km. Verificar tu correo te permite recuperar la contraseña.")
            BotonPrincipal("Ir al mapa", alIrAlMapa)
            TextButton(onClick = alReenviar, enabled = !reenviado) {
                Text(if (reenviado) "Correo reenviado" else "Reenviar el correo", fontWeight = FontWeight.Bold)
            }
        }
    }
}

/** Sobre dentro de un círculo suave (prototipo `.hero-ic`); decorativo, TalkBack lo salta. */
@Composable
private fun IconoCorreo() {
    val paleta = KillaUruTema.paleta
    Box(Modifier.size(89.dp).background(paleta.killaSuave, CircleShape), contentAlignment = Alignment.Center) {
        Icon(IconosKillaUru.de("mail"), contentDescription = null, tint = paleta.killa, modifier = Modifier.size(34.dp))
    }
}

/** Une [VerificarCorreoPantalla] con su ViewModel. */
@Composable
internal fun VerificarCorreoRuta(correo: String, alIrAlMapa: () -> Unit, vm: VerificarCorreoViewModel = hiltViewModel()) {
    val reenviado by vm.reenviado.collectAsState()
    VerificarCorreoPantalla(correo, reenviado, alIrAlMapa, vm::reenviar)
}
