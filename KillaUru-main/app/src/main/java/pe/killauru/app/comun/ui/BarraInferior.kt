package pe.killauru.app.comun.ui

import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable

/** FR-008: barra de navegación inferior con Mapa, Mis viajes y Ajustes, con los íconos del prototipo. */
@Composable
internal fun BarraInferior(
    rutaActual: String,
    onMapa: () -> Unit,
    onHistorial: () -> Unit,
    onAjustes: () -> Unit,
) {
    NavigationBar {
        ElementoBarra(rutaActual == Rutas.Mapa.ruta, onMapa, IconosKillaUru.de("map"), "Mapa")
        ElementoBarra(rutaActual == Rutas.Historial.ruta, onHistorial, IconosKillaUru.de("hist"), "Mis viajes")
        ElementoBarra(rutaActual == Rutas.Ajustes.ruta, onAjustes, IconosKillaUru.de("gear"), "Ajustes")
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
