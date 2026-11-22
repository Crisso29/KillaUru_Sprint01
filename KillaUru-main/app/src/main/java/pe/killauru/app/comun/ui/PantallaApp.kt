package pe.killauru.app.comun.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.foundation.layout.imePadding
import androidx.compose.ui.unit.dp

/** Paso actual del primer uso («Paso 2 de 5», prototipo `.steps`). */
internal data class Paso(val actual: Int, val total: Int)

/**
 * Pantalla del prototipo (`.scr`): encabezado con atrás y título (`.appbar`), pasos opcionales, cuerpo desplazable
 * (`.body`, 21 dp de margen) y pie fijo con los botones (`.foot`). Todas las pantallas de formulario de la app la usan.
 */
@Composable
internal fun PantallaApp(
    titulo: String,
    modifier: Modifier = Modifier,
    alVolver: (() -> Unit)? = null,
    paso: Paso? = null,
    pie: (@Composable ColumnScope.() -> Unit)? = null,
    contenido: @Composable ColumnScope.() -> Unit,
) {
    Surface(modifier = modifier.fillMaxSize()) {
        // imePadding: con el teclado abierto, el pie con el botón sube sobre él (Android 15 dibuja de borde a borde).
        Column(Modifier.fillMaxSize().imePadding()) {
            Encabezado(titulo, alVolver)
            if (paso != null) IndicadorPasos(paso)
            Column(
                modifier = Modifier.weight(1f).fillMaxWidth().verticalScroll(rememberScrollState())
                    .padding(start = 21.dp, end = 21.dp, top = 13.dp, bottom = 21.dp),
                verticalArrangement = Arrangement.spacedBy(13.dp),
                content = contenido,
            )
            if (pie != null) Pie(pie)
        }
    }
}

/** Pie fijo con los botones (`.foot`): siempre a la vista, fuera de lo que se desplaza. */
@Composable
private fun Pie(contenido: @Composable ColumnScope.() -> Unit) {
    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
    Column(
        modifier = Modifier.fillMaxWidth().padding(start = 21.dp, end = 21.dp, top = 13.dp, bottom = 21.dp),
        verticalArrangement = Arrangement.spacedBy(5.dp),
        content = contenido,
    )
}

@Composable
private fun Encabezado(titulo: String, alVolver: (() -> Unit)?) {
    Row(
        modifier = Modifier.fillMaxWidth().heightIn(min = 55.dp).padding(horizontal = 8.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(5.dp),
    ) {
        if (alVolver != null) {
            IconButton(onClick = alVolver) { Icon(IconosKillaUru.de("back"), contentDescription = "Volver") }
        } else {
            Box(Modifier.size(13.dp))
        }
        Text(titulo, style = MaterialTheme.typography.titleLarge, modifier = Modifier.semantics { heading() })
    }
}

/** «Paso 2 de 5» y cinco segmentos, los hechos en color de marca (prototipo `.steps`). */
@Composable
private fun IndicadorPasos(paso: Paso) {
    Row(
        modifier = Modifier.fillMaxWidth().padding(horizontal = 21.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(5.dp),
    ) {
        Text("Paso ${paso.actual} de ${paso.total}", style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant)
        repeat(paso.total) { i ->
            val color = if (i < paso.actual) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
            Box(Modifier.weight(1f).height(3.dp).background(color, RoundedCornerShape(2.dp)))
        }
    }
}
