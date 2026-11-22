package pe.killauru.app.comun.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import pe.killauru.app.comun.ui.tema.KillaUruTema
import pe.killauru.app.comun.ui.tema.Paleta

/** Tono del aviso (prototipo `.note.killa|info|warn|bad|ok`). */
internal enum class TonoAviso { KILLA, INFO, AVISO, MAL, OK }

private fun Paleta.fondo(tono: TonoAviso): Color = when (tono) {
    TonoAviso.KILLA -> killaSuave
    TonoAviso.INFO -> infoSuave
    TonoAviso.AVISO -> avisoSuave
    TonoAviso.MAL -> malSuave
    TonoAviso.OK -> okSuave
}

/** Caja de aviso con ícono y texto, sobre el fondo suave de su tono (prototipo `.note`). */
@Composable
internal fun Aviso(icono: String, tono: TonoAviso, modifier: Modifier = Modifier, contenido: @Composable () -> Unit) {
    Row(
        modifier = modifier.fillMaxWidth()
            .background(KillaUruTema.paleta.fondo(tono), RoundedCornerShape(8.dp))
            .padding(horizontal = 13.dp, vertical = 8.dp),
        horizontalArrangement = Arrangement.spacedBy(8.dp),
        verticalAlignment = Alignment.Top,
    ) {
        Icon(IconosKillaUru.de(icono), contentDescription = null, modifier = Modifier.size(20.dp).padding(top = 1.dp),
            tint = MaterialTheme.colorScheme.onSurface)
        contenido()
    }
}

/** Atajo: aviso de un solo texto. */
@Composable
internal fun Aviso(icono: String, tono: TonoAviso, texto: String, modifier: Modifier = Modifier) =
    Aviso(icono, tono, modifier) { Text(texto, style = MaterialTheme.typography.bodyMedium) }
