package pe.killauru.app.deteccion.ui

import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import pe.killauru.app.deteccion.dominio.IndicadorEstado

/** FR-016, FR-008: chip que muestra el estado de detección en la pantalla. */
@Composable
internal fun IndicadorEstadoChip(indicador: IndicadorEstado, pendientes: Int = 0) {
    val etiqueta = when (indicador) {
        IndicadorEstado.Detectando ->
            if (pendientes > 0) "Detectando · $pendientes por enviar" else "Detectando"
        IndicadorEstado.EnPausa -> "En pausa"
        IndicadorEstado.SinGps -> "Sin GPS"
        IndicadorEstado.ModoAhorro -> "Ahorro de batería"
        IndicadorEstado.SoloMapa -> "Solo ver el mapa"
        IndicadorEstado.SinPermiso -> "Sin permiso"
        IndicadorEstado.SinServicio -> "Servicio detenido"
    }
    AssistChip(onClick = {}, label = { Text(etiqueta) })
}
