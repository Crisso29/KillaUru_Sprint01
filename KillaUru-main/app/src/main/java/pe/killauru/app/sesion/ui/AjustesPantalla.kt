package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalUriHandler
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.semantics.heading
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.R
import pe.killauru.app.comun.ui.FilaAjuste
import pe.killauru.app.comun.ui.IconosKillaUru
import pe.killauru.app.comun.ui.TituloSeccion
import pe.killauru.app.comun.ui.tema.FuenteDatos

/**
 * FR-005, FR-007, FR-008 (prototipo «Ajustes»): todo lo configurable, agrupado por lo que la persona quiere hacer:
 * detección, mapa sin conexión y cuenta y privacidad.
 */
@Composable
internal fun AjustesPantalla(
    onVerTerminos: () -> Unit = {},
    onVerPerfil: () -> Unit = {},
    onCrearCuenta: () -> Unit = {},
    onIniciarSesion: () -> Unit = {},
    viewModel: AjustesViewModel = hiltViewModel(),
) {
    val estado by viewModel.estado.collectAsState()
    LaunchedEffect(Unit) { viewModel.recargar() } // al volver de iniciar o cerrar sesión
    Surface(Modifier.fillMaxSize()) {
        Column(Modifier.verticalScroll(rememberScrollState()).padding(horizontal = 21.dp, vertical = 13.dp)) {
            Text("Ajustes", style = MaterialTheme.typography.headlineMedium, modifier = Modifier.semantics { heading() })
            Deteccion(estado, viewModel)
            MapaSinConexion(estado, viewModel)
            CuentaYPrivacidad(estado, AccionesCuenta(onVerPerfil, onCrearCuenta, onIniciarSesion), onVerTerminos)
        }
    }
}

@Composable
private fun Deteccion(estado: EstadoAjustes, viewModel: AjustesViewModel) {
    var eligiendo by rememberSaveable { mutableStateOf(false) }
    TituloSeccion("Detección")
    FilaAjuste("Mi vehículo", detalle = "Solo Liviano detecta y envía", alPulsar = { eligiendo = !eligiendo }) {
        Text(estado.categoria?.textos()?.nombre ?: "Sin elegir", color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
    if (eligiendo) {
        SelectorVehiculo(estado.categoria) { viewModel.cambiarCategoria(it); eligiendo = false }
    }
    FilaAjuste("Solo ver el mapa", detalle = "No detecta ni envía. Sigues viendo el mapa y las alertas.") {
        Switch(checked = estado.soloVerMapa, onCheckedChange = viewModel::cambiarSoloVerMapa)
    }
    FilaAjuste("Modelo de detección", detalle = "Se actualiza con Wi-Fi, nunca a mitad de un viaje") {
        Text(estado.modeloVersion?.let { "v$it" } ?: "—", fontFamily = FuenteDatos, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

@Composable
private fun MapaSinConexion(estado: EstadoAjustes, viewModel: AjustesViewModel) {
    TituloSeccion("Mapa sin conexión")
    FilaAjuste("Corredor PE-28A", detalle = estado.mapaVersion?.let { "Versión $it" } ?: "Aún no descargado")
    FilaAjuste("Usar datos móviles", detalle = "Para descargar sin Wi-Fi") {
        Switch(checked = estado.usarDatosMoviles, onCheckedChange = viewModel::cambiarUsarDatosMoviles)
    }
}

@Composable
private fun CuentaYPrivacidad(estado: EstadoAjustes, cuenta: AccionesCuenta, onVerTerminos: () -> Unit) {
    val abrir = LocalUriHandler.current
    val urlEstado = stringResource(R.string.url_estado_servicio)
    TituloSeccion("Cuenta y privacidad")
    if (estado.conCuenta) {
        FilaAjuste("Mi perfil", detalle = "Tu cuenta y tus datos", alPulsar = cuenta.verPerfil)
    } else {
        FilaAjuste("Crear cuenta", detalle = "Gratis: alertas en ruta y tu historial de viajes", alPulsar = cuenta.crear)
        FilaAjuste("Iniciar sesión", detalle = "Si ya tienes una cuenta", alPulsar = cuenta.iniciarSesion)
    }
    FilaAjuste("Términos y privacidad", detalle = textoAceptacion(estado), alPulsar = onVerTerminos)
    FilaAjuste("Estado del servicio", detalle = "¿Funciona KillaUru ahora?", alPulsar = { abrir.openUri(urlEstado) }) {
        Icon(IconosKillaUru.de("ext"), contentDescription = "Abre en el navegador", modifier = Modifier.size(18.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

/** A dónde llevan las filas de cuenta: perfil con sesión; crear o iniciar sesión sin ella. */
private class AccionesCuenta(val verPerfil: () -> Unit, val crear: () -> Unit, val iniciarSesion: () -> Unit)

/** «Aceptaste la v1.2 el 14 oct 2026» (FR-007), en hora de Lima. */
internal fun textoAceptacion(estado: EstadoAjustes): String {
    val version = estado.terminosVersion ?: return "Aún no aceptados"
    val fecha = estado.terminosAceptadoUtcMs?.let { FechaCorta.deUtcMs(it) } ?: return "Aceptaste la v$version"
    return "Aceptaste la v$version el $fecha"
}
