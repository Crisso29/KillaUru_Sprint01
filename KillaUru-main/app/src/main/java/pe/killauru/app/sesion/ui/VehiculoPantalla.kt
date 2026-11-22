package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.comun.ui.MarcoFormulario
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/**
 * FR-004, HU-26, RN-18 (prototipo «¿Qué manejas?»): elegir la categoría, con la guía del soporte y del cargador.
 * La categoría se guarda solo en el celular; «Continuar» se activa al elegir una.
 */
@Composable
internal fun VehiculoPantalla(onSeleccionar: (CategoriaVehiculo) -> Unit) {
    var seleccion by rememberSaveable { mutableStateOf<CategoriaVehiculo?>(null) }
    MarcoFormulario(desplazable = true) {
        Text("¿Qué manejas?", style = MaterialTheme.typography.headlineMedium)
        Spacer(Modifier.height(8.dp))
        Text("Se guarda solo en este celular. Puedes cambiarlo en Ajustes.", style = MaterialTheme.typography.bodyMedium)
        Spacer(Modifier.height(13.dp))
        SelectorVehiculo(seleccion) { seleccion = it }
        Spacer(Modifier.height(13.dp))
        GuiaSoporte()
        Spacer(Modifier.height(21.dp))
        Button(
            onClick = { seleccion?.let(onSeleccionar) },
            enabled = seleccion != null,
            modifier = Modifier.fillMaxWidth(),
        ) { Text("Continuar") }
    }
}

/** Sin soporte fijo el acelerómetro mide el bolsillo, no la vía; en turnos largos el GPS a 1 Hz gasta batería. */
@Composable
private fun GuiaSoporte() {
    Text("Celular en un soporte fijo.", style = MaterialTheme.typography.titleMedium)
    Text(
        "En el bolsillo no detecta bien. En turnos largos, conéctalo al cargador.",
        style = MaterialTheme.typography.bodyMedium,
    )
    Spacer(Modifier.height(8.dp))
    Text(
        "Las alertas son para quienes tienen cuenta, con cualquier vehículo.",
        style = MaterialTheme.typography.bodySmall,
        color = MaterialTheme.colorScheme.onSurfaceVariant,
    )
}
