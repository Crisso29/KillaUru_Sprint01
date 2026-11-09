package pe.killauru.app.comun.ui

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** Contenedor compartido de formularios, con espaciado y desplazamiento consistentes. */
@Composable
internal fun MarcoFormulario(
    centrado: Boolean = false,
    alineado: Boolean = false,
    desplazable: Boolean = false,
    contenido: @Composable ColumnScope.() -> Unit,
) {
    val base = Modifier.fillMaxSize().padding(16.dp)
    val modificador = if (desplazable) base.verticalScroll(rememberScrollState()) else base
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = modificador,
            horizontalAlignment = if (alineado) Alignment.CenterHorizontally else Alignment.Start,
            verticalArrangement = if (centrado) Arrangement.Center else Arrangement.Top,
            content = contenido,
        )
    }
}
