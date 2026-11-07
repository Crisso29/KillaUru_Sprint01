package pe.killauru.app.deteccion.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import pe.killauru.app.deteccion.dominio.IndicadorEstado

/** FR-016, FR-008: pantalla de estado del servicio de detección. */
@Composable
internal fun EstadosPantalla(
    indicador: IndicadorEstado = IndicadorEstado.SinServicio,
    pendientes: Int = 0,
    onDetener: () -> Unit = {},
) {
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            IndicadorEstadoChip(indicador, pendientes)
            Spacer(Modifier.height(24.dp))
            if (indicador != IndicadorEstado.SinServicio) {
                OutlinedButton(onClick = onDetener) {
                    Text("Detener")
                }
            }
        }
    }
}
