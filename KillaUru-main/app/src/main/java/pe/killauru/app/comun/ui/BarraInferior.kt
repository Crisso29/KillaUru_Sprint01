package pe.killauru.app.comun.ui

import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Text
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.LocationOn
import androidx.compose.material.icons.filled.History
import androidx.compose.material.icons.filled.Settings
import androidx.compose.runtime.Composable

/** FR-008: barra de navegación inferior con Mapa, Historial y Ajustes. */
@Composable
internal fun BarraInferior(
    rutaActual: String,
    onMapa: () -> Unit,
    onHistorial: () -> Unit,
    onAjustes: () -> Unit,
) {
    NavigationBar {
        NavigationBarItem(
            selected = rutaActual == Rutas.Mapa.ruta,
            onClick = onMapa,
            icon = { Icon(Icons.Default.LocationOn, contentDescription = "Mapa") },
            label = { Text("Mapa") },
        )
        NavigationBarItem(
            selected = rutaActual == Rutas.Historial.ruta,
            onClick = onHistorial,
            icon = { Icon(Icons.Default.History, contentDescription = "Mis viajes") },
            label = { Text("Mis viajes") },
        )
        NavigationBarItem(
            selected = rutaActual == Rutas.Ajustes.ruta,
            onClick = onAjustes,
            icon = { Icon(Icons.Default.Settings, contentDescription = "Ajustes") },
            label = { Text("Ajustes") },
        )
    }
}
