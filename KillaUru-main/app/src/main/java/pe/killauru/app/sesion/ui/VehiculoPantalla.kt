package pe.killauru.app.sesion.ui

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

    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
        ) {
            Text("¿Qué tipo de vehículo usas?", style = MaterialTheme.typography.headlineSmall)
            Spacer(Modifier.height(8.dp))
            Text(
                "Usamos esta información para ajustar la sensibilidad de la detección.",
                style = MaterialTheme.typography.bodyMedium,
            )
            Spacer(Modifier.height(24.dp))
            CategoriaVehiculo.entries.forEach { cat ->
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
                ) {
                    RadioButton(
                        selected = seleccion == cat,
                        onClick = { seleccion = cat },
                    )
                    Spacer(Modifier.width(8.dp))
                    Text(cat.texto, style = MaterialTheme.typography.bodyLarge)
                }
            }
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
}
