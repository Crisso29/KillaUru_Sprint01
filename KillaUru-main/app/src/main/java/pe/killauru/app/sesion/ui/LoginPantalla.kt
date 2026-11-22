package pe.killauru.app.sesion.ui

import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.text.font.FontWeight
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.CampoFormulario
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.TipoCampo
import pe.killauru.app.comun.ui.TonoAviso

/**
 * FR-061, FR-063, RN-19 (prototipo «Iniciar sesión»): el mismo mensaje si el correo no existe o la contraseña es
 * incorrecta, para no revelar quién tiene cuenta.
 */
@Composable
internal fun LoginPantalla(
    estado: LoginUiState,
    alEntrar: (correo: String, contrasena: String) -> Unit,
    alOlvide: () -> Unit,
    alVolver: (() -> Unit)? = null,
) {
    var correo by rememberSaveable { mutableStateOf("") }
    var contrasena by rememberSaveable { mutableStateOf("") }
    val listo = !estado.cargando && correo.isNotBlank() && contrasena.isNotBlank()
    val entrar = { if (listo) alEntrar(correo.trim(), contrasena) }
    PantallaApp(
        titulo = "Iniciar sesión",
        alVolver = alVolver,
        pie = { BotonPrincipal(if (estado.cargando) "Entrando…" else "Entrar", entrar, habilitado = listo) },
    ) {
        estado.error?.let { Aviso("warn", TonoAviso.AVISO, it) }
        CampoFormulario("Correo", correo, { correo = it }, TipoCampo.CORREO)
        CampoFormulario("Contraseña", contrasena, { contrasena = it }, TipoCampo.CONTRASENA, alConfirmar = entrar)
        TextButton(onClick = alOlvide) { Text("Olvidé mi contraseña", fontWeight = FontWeight.Bold) }
    }
}

/** Une [LoginPantalla] con su ViewModel; al entrar avisa a la navegación. */
@Composable
internal fun LoginRuta(alEntrado: () -> Unit, alOlvide: () -> Unit, alVolver: () -> Unit, vm: LoginViewModel = hiltViewModel()) {
    val estado by vm.estado.collectAsState()
    LoginPantalla(estado, { c, p -> vm.iniciarSesion(c, p, alEntrado) }, alOlvide, alVolver)
}
