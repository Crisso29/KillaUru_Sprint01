package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.selection.selectableGroup
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.comun.ui.TarjetaOpcion
import pe.killauru.app.comun.ui.tema.KillaUruTema
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

private val ICONO = mapOf(CategoriaVehiculo.LIVIANO to "car", CategoriaVehiculo.PESADO to "truck", CategoriaVehiculo.MOTO to "moto")

/** FR-004, RN-18: las tres categorías como tarjetas del prototipo (`.opt`); lo usan el primer uso y Ajustes. */
@Composable
internal fun SelectorVehiculo(seleccion: CategoriaVehiculo?, onSeleccionar: (CategoriaVehiculo) -> Unit) {
    Column(Modifier.selectableGroup(), verticalArrangement = Arrangement.spacedBy(8.dp)) {
        CategoriaVehiculo.entries.forEach { categoria ->
            val t = categoria.textos()
            TarjetaOpcion(
                icono = ICONO.getValue(categoria),
                titulo = t.nombre,
                descripcion = t.ejemplos,
                efecto = t.efecto,
                colorEfecto = if (t.detecta) KillaUruTema.paleta.ok else MaterialTheme.colorScheme.onSurfaceVariant,
                elegida = seleccion == categoria,
                alElegir = { onSeleccionar(categoria) },
            )
        }
    }
}
