package pe.killauru.app.sesion.ui

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.withStyle
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.Paso
import pe.killauru.app.comun.ui.TonoAviso
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** Número de pasos del primer uso en el prototipo (términos, vehículo, permisos, descarga del mapa y cuenta). */
internal const val PASOS_PRIMER_USO = 5

/**
 * FR-004, HU-26, RN-18 (prototipo «¿Qué manejas?», paso 2 de 5): cada opción dice qué pasará, con la guía del soporte y
 * del cargador. La categoría se guarda solo en el celular; «Continuar» se activa al elegir una.
 */
@Composable
internal fun VehiculoPantalla(onSeleccionar: (CategoriaVehiculo) -> Unit, alVolver: (() -> Unit)? = null) {
    var seleccion by rememberSaveable { mutableStateOf<CategoriaVehiculo?>(null) }
    PantallaApp(
        titulo = "¿Qué manejas?",
        alVolver = alVolver,
        paso = Paso(2, PASOS_PRIMER_USO),
        pie = { BotonPrincipal("Continuar", { seleccion?.let(onSeleccionar) }, habilitado = seleccion != null) },
    ) {
        Text("Se guarda solo en este celular. Puedes cambiarlo en Ajustes.", style = MaterialTheme.typography.bodyLarge,
            color = MaterialTheme.colorScheme.onSurfaceVariant)
        SelectorVehiculo(seleccion) { seleccion = it }
        GuiaSoporte()
        Text("Las alertas son para quienes tienen cuenta, con cualquier vehículo.",
            style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

/** Sin soporte fijo el acelerómetro mide el bolsillo, no la vía; en turnos largos el GPS a 1 Hz gasta batería. */
@Composable
private fun GuiaSoporte() = Aviso("phone", TonoAviso.KILLA) {
    Text(buildAnnotatedString {
        withStyle(SpanStyle(fontWeight = FontWeight.Bold)) { append("Celular en un soporte fijo.") }
        append(" En el bolsillo no detecta bien. En turnos largos, conéctalo al cargador.")
    }, style = MaterialTheme.typography.bodyMedium)
}
