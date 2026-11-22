package pe.killauru.app.comun.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.selection.selectable
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.foundation.layout.RowScope
import pe.killauru.app.comun.ui.tema.KillaUruTema

private val FORMA = RoundedCornerShape(13.dp)

/**
 * Opción grande del prototipo (`.opt`): ícono en un recuadro, título, descripción y qué pasará si la eliges (`tag`).
 * Toda la tarjeta es un solo objetivo de toque (rol RadioButton); elegida, toma el borde y el fondo de la marca.
 */
@Composable
internal fun TarjetaOpcion(
    icono: String,
    titulo: String,
    descripcion: String,
    elegida: Boolean,
    alElegir: () -> Unit,
    modifier: Modifier = Modifier,
    efecto: String? = null,
    colorEfecto: Color = MaterialTheme.colorScheme.onSurfaceVariant,
) {
    val paleta = KillaUruTema.paleta
    Row(
        modifier = modifier.fillMaxWidth().clip(FORMA)
            .background(if (elegida) paleta.killaSuave else MaterialTheme.colorScheme.surface)
            .border(1.5.dp, if (elegida) paleta.killa else paleta.linea, FORMA)
            .selectable(selected = elegida, role = Role.RadioButton, onClick = alElegir)
            .padding(13.dp),
        horizontalArrangement = Arrangement.spacedBy(13.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        IconoOpcion(icono)
        TextosOpcion(titulo, descripcion, efecto, colorEfecto)
    }
}

/** Título, descripción y, si lo hay, el efecto de elegirla (p. ej. «Detecta y envía»). */
@Composable
private fun RowScope.TextosOpcion(titulo: String, descripcion: String, efecto: String?, colorEfecto: Color) {
    Column(Modifier.weight(1f)) {
        Text(titulo, style = MaterialTheme.typography.titleLarge.copy(fontSize = 18.sp))
        Text(descripcion, style = MaterialTheme.typography.bodyMedium.copy(fontSize = 14.sp),
            color = MaterialTheme.colorScheme.onSurfaceVariant)
        if (efecto != null) {
            Text(efecto, fontSize = 13.sp, fontWeight = FontWeight.Bold, color = colorEfecto, modifier = Modifier.padding(top = 2.dp))
        }
    }
}

/** Icono en un cuadro hundido (`.opt-ic`); decorativo: el título ya dice qué es. */
@Composable
private fun IconoOpcion(icono: String) {
    val paleta = KillaUruTema.paleta
    Box(Modifier.size(48.dp).background(paleta.hundido, RoundedCornerShape(13.dp)), contentAlignment = Alignment.Center) {
        Icon(IconosKillaUru.de(icono), contentDescription = null, modifier = Modifier.size(28.dp), tint = paleta.tinta)
    }
}
