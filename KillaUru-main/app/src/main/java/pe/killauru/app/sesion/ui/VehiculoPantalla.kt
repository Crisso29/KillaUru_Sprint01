package pe.killauru.app.sesion.ui

import pe.killauru.app.comun.ui.MarcoFormulario

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** FR-004, FR-008: pantalla de selección de tipo de vehículo. */
@Composable
internal fun VehiculoPantalla(
    onSeleccionar: (CategoriaVehiculo) -> Unit,
) {
    var seleccion by remember { mutableStateOf<CategoriaVehiculo?>(null) }

    MarcoFormulario() {
            ExplicacionVehiculo()
            SelectorVehiculo(seleccion) { seleccion = it }
            Spacer(Modifier.weight(1f))
            Button(
                onClick = { seleccion?.let(onSeleccionar) },
                enabled = seleccion != null,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text("Continuar")
            }
    }
}

@Composable
private fun ExplicacionVehiculo() {
            Text("¿Qué tipo de vehículo usas?", style = MaterialTheme.typography.headlineSmall)
            Spacer(Modifier.height(8.dp))
            Text(
                "Usamos esta información para ajustar la sensibilidad de la detección.",
                style = MaterialTheme.typography.bodyMedium,
            )
            Spacer(Modifier.height(24.dp))
}
