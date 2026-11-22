package pe.killauru.app.sesion.ui

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.CampoFormulario
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.TipoCampo
import pe.killauru.app.comun.ui.TonoAviso

/**
 * HU-25 (prototipo «Recuperar contraseña»): pide el enlace; la respuesta es la misma exista o no la cuenta. Tras el envío,
 * el botón principal vuelve al inicio de sesión.
 */
@Composable
internal fun OlvideContrasenaPantalla(
    estado: OlvideUiState,
    alEnviar: (correo: String) -> Unit,
    alVolverAlLogin: () -> Unit,
    alVolver: (() -> Unit)? = null,
) {
    var correo by rememberSaveable { mutableStateOf("") }
    PantallaApp(
        titulo = "Recuperar contraseña",
        alVolver = alVolver,
        pie = { PieOlvide(estado, { alEnviar(correo) }, correo.isNotBlank(), alVolverAlLogin) },
    ) {
        Text("Escribe tu correo. Si tiene una cuenta verificada, te enviamos un enlace.",
            style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.onSurfaceVariant)
        CampoFormulario("Correo", correo, { correo = it }, TipoCampo.CORREO,
            alConfirmar = { if (!estado.cargando && !estado.enviado && correo.isNotBlank()) alEnviar(correo) })
        if (estado.enviado) {
            Aviso("check", TonoAviso.OK, "Listo. Si el correo está registrado, te llegará un enlace que vence en 30 minutos. " +
                "Al cambiar la contraseña se cierran tus otras sesiones.")
        }
        estado.error?.let { Aviso("warn", TonoAviso.AVISO, it) }
    }
}

/** Antes del envío, «Enviar enlace»; después, «Volver a iniciar sesión». */
@Composable
private fun PieOlvide(estado: OlvideUiState, alEnviar: () -> Unit, conCorreo: Boolean, alVolverAlLogin: () -> Unit) {
    if (estado.enviado) {
        BotonPrincipal("Volver a iniciar sesión", alVolverAlLogin)
    } else {
        BotonPrincipal(if (estado.cargando) "Enviando…" else "Enviar enlace", alEnviar, habilitado = !estado.cargando && conCorreo)
    }
}

/** Une [OlvideContrasenaPantalla] con su ViewModel. */
@Composable
internal fun OlvideContrasenaRuta(alVolverAlLogin: () -> Unit, alVolver: () -> Unit, vm: OlvideContrasenaViewModel = hiltViewModel()) {
    val estado by vm.estado.collectAsState()
    OlvideContrasenaPantalla(estado, vm::enviar, alVolverAlLogin, alVolver)
}
