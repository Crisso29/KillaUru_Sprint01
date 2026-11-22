package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.selection.selectableGroup
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.comun.ui.FilaOpcion
import pe.killauru.app.comun.ui.tema.KillaUruTema
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** FR-004, RN-18: las tres categorías como filas pulsables; lo usan el primer uso y Ajustes. */
@Composable
internal fun SelectorVehiculo(seleccion: CategoriaVehiculo?, onSeleccionar: (CategoriaVehiculo) -> Unit) {
    Column(Modifier.selectableGroup()) {
        CategoriaVehiculo.entries.forEach { categoria ->
            FilaOpcion(elegida = seleccion == categoria, alElegir = { onSeleccionar(categoria) }) {
                Spacer(Modifier.width(13.dp))
                Opcion(categoria.textos())
            }
        }
    }
}

@Composable
private fun Opcion(t: TextosVehiculo) = Column {
    Text(t.nombre, style = MaterialTheme.typography.titleLarge)
    Text(t.ejemplos, style = MaterialTheme.typography.bodyMedium)
    Text(
        t.efecto,
        style = MaterialTheme.typography.labelMedium,
        color = if (t.detecta) KillaUruTema.paleta.ok else MaterialTheme.colorScheme.onSurfaceVariant,
    )
}
