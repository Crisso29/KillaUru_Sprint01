package pe.killauru.app.comun.ui

import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.selection.selectable
import androidx.compose.foundation.selection.toggleable
import androidx.compose.material3.Checkbox
import androidx.compose.material3.RadioButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.unit.dp

/**
 * Filas pulsables enteras para casillas y opciones: el conductor puede tocar el texto, no solo el control de 20 dp
 * (pauta de Material y de accesibilidad: un solo objetivo, mínimo 48 dp de alto). El control no recibe `onClick`
 * propio para que la fila entera sea una sola acción para TalkBack.
 */
@Composable
internal fun FilaCasilla(marcada: Boolean, alCambiar: (Boolean) -> Unit, contenido: @Composable RowScope.() -> Unit) =
    Fila(Modifier.toggleable(value = marcada, role = Role.Checkbox, onValueChange = alCambiar)) {
        Checkbox(checked = marcada, onCheckedChange = null)
        contenido()
    }

@Composable
internal fun FilaOpcion(elegida: Boolean, alElegir: () -> Unit, contenido: @Composable RowScope.() -> Unit) =
    Fila(Modifier.selectable(selected = elegida, role = Role.RadioButton, onClick = alElegir)) {
        RadioButton(selected = elegida, onClick = null)
        contenido()
    }

@Composable
private fun Fila(modifier: Modifier, contenido: @Composable RowScope.() -> Unit) = Row(
    verticalAlignment = Alignment.CenterVertically,
    modifier = Modifier.fillMaxWidth().heightIn(min = 48.dp).then(modifier).padding(vertical = 8.dp),
    content = contenido,
)
