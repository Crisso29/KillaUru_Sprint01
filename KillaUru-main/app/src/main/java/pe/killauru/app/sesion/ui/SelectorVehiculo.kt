package pe.killauru.app.sesion.ui


import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

@Composable
internal fun SelectorVehiculo(seleccion: CategoriaVehiculo?, onSeleccionar: (CategoriaVehiculo) -> Unit) {
            CategoriaVehiculo.entries.forEach { cat ->
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
                ) {
                    RadioButton(
                        selected = seleccion == cat,
                        onClick = { onSeleccionar(cat) },
                    )
                    Spacer(Modifier.width(8.dp))
                    Text(cat.texto, style = MaterialTheme.typography.bodyLarge)
                }
            }
}
