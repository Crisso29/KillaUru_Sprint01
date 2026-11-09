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
        ElementoBarra(rutaActual == Rutas.Mapa.ruta, onMapa, Icons.Default.LocationOn, "Mapa")
        ElementoBarra(rutaActual == Rutas.Historial.ruta, onHistorial, Icons.Default.History, "Mis viajes")
        ElementoBarra(rutaActual == Rutas.Ajustes.ruta, onAjustes, Icons.Default.Settings, "Ajustes")
    }
}

@Composable
private fun androidx.compose.foundation.layout.RowScope.ElementoBarra(
    seleccionado: Boolean, onClick: () -> Unit,
    icono: androidx.compose.ui.graphics.vector.ImageVector, texto: String,
) {
    NavigationBarItem(selected = seleccionado, onClick = onClick,
        icon = { Icon(icono, contentDescription = texto) }, label = { Text(texto) })
}
