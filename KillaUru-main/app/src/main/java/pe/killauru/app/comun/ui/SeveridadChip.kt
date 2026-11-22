package pe.killauru.app.comun.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import pe.killauru.app.comun.ui.tema.KillaUruTema

/** FR-008, RN-02: círculo de severidad del prototipo (`.sev`), con el color de su nivel; se lee «Severidad N». */
@Composable
internal fun SeveridadChip(severidad: Int, modifier: Modifier = Modifier) {
    val paleta = KillaUruTema.paleta
    Box(
        contentAlignment = Alignment.Center,
        modifier = modifier
            .size(26.dp)
            .background(paleta.colorSeveridad(severidad), CircleShape)
            .semantics { contentDescription = "Severidad $severidad" },
    ) {
        Text(
            "$severidad",
            color = paleta.textoSobreSeveridad(severidad),
            style = MaterialTheme.typography.titleSmall,
        )
    }
}
