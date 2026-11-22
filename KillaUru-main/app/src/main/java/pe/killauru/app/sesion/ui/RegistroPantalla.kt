package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.CampoFormulario
import pe.killauru.app.comun.ui.FilaCasilla
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.TipoCampo
import pe.killauru.app.comun.ui.TonoAviso

private const val INTRO_REGISTRO = "Gratis. Solo tu correo, y nunca lo unimos con tus viajes ni con lo que detectas."

/**
 * FR-060, FR-065, HU-18 (prototipo «Crear cuenta»): solo correo y contraseña (mínimo 10 caracteres) y el consentimiento
 * explícito; explica qué se guarda. Nunca se une la cuenta a los viajes ni a lo detectado (RNF-10).
 */
@Composable
internal fun RegistroPantalla(
    estado: RegistroUiState,
    alCrear: (correo: String, contrasena: String) -> Unit,
    alVolver: (() -> Unit)? = null,
) {
    var correo by rememberSaveable { mutableStateOf("") }
    var contrasena by rememberSaveable { mutableStateOf("") }
    var acepto by rememberSaveable { mutableStateOf(false) }
    PantallaApp(
        titulo = "Crear cuenta",
        alVolver = alVolver,
        pie = {
            BotonPrincipal(if (estado.cargando) "Creando…" else "Crear cuenta", { alCrear(correo.trim(), contrasena) },
                habilitado = !estado.cargando && acepto && correo.isNotBlank() && contrasena.isNotBlank())
        },
    ) {
        Text(INTRO_REGISTRO, style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.onSurfaceVariant)
        estado.error?.let { Aviso("warn", TonoAviso.AVISO, it) }
        CampoFormulario("Correo", correo, { correo = it }, TipoCampo.CORREO)
        CampoFormulario("Contraseña", contrasena, { contrasena = it }, TipoCampo.CONTRASENA_NUEVA,
            ayuda = "Mínimo 10 caracteres. Una frase fácil de recordar sirve.")
        Consentimiento(acepto, estado.politicaVersion) { acepto = it }
    }
}

/** FR-003: consentimiento explícito y qué se guarda al aceptarlo (Ley 29733). */
@Composable
private fun Consentimiento(acepto: Boolean, version: String?, alCambiar: (Boolean) -> Unit) {
    FilaCasilla(acepto, alCambiar) {
        Spacer(Modifier.width(8.dp))
        Text("Acepto la política de privacidad${version?.let { " (v$it)" }.orEmpty()} y los términos de uso")
    }
    Text("Guardamos tu correo, tu contraseña cifrada y la fecha en que aceptaste. Puedes verlo o borrarlo cuando quieras.",
        style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
}

/** Une [RegistroPantalla] con su ViewModel; creada la cuenta, pasa a «Revisa tu correo» con ese correo. */
@Composable
internal fun RegistroRuta(alCreada: (correo: String) -> Unit, alVolver: () -> Unit, vm: RegistroViewModel = hiltViewModel()) {
    val estado by vm.estado.collectAsState()
    RegistroPantalla(estado, { c, p -> vm.registrar(c, p) { alCreada(c) } }, alVolver)
}
