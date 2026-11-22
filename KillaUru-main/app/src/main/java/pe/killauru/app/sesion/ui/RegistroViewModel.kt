package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.casos.RegistrarCuentaUseCase
import javax.inject.Inject

/** [politicaVersion]: la que aceptó en el primer uso y se enviará al registrarse (prototipo «Acepto la política (v1.2)»). */
internal data class RegistroUiState(val error: String? = null, val cargando: Boolean = false, val politicaVersion: String? = null)

@HiltViewModel
internal class RegistroViewModel @Inject constructor(
    private val registrar: RegistrarCuentaUseCase,
    private val prefs: RepositorioPreferencias,
) : ViewModel() {

    private val _estado = MutableStateFlow(RegistroUiState())
    val estado: StateFlow<RegistroUiState> = _estado

    init {
        viewModelScope.launch { _estado.value = _estado.value.copy(politicaVersion = prefs.obtenerPoliticaVersion()) }
    }

    fun registrar(correo: String, contrasena: String, onRegistrado: () -> Unit) {
        viewModelScope.launch {
            _estado.value = _estado.value.copy(cargando = true, error = null)
            val resultado = registrar.ejecutar(correo, contrasena)
            if (resultado is ResultadoCuenta.Exitoso) {
                _estado.value = _estado.value.copy(cargando = false)
                onRegistrado()
            } else {
                _estado.value = _estado.value.copy(cargando = false, error = mensaje(resultado))
            }
        }
    }

    /** FR-060, D13: mensajes del registro; 409 sugiere iniciar sesión o recuperar la contraseña. */
    private fun mensaje(resultado: ResultadoCuenta): String = when (resultado) {
        ResultadoCuenta.CorreoEnUso -> "Este correo ya tiene cuenta. Inicia sesión o recupera tu contraseña."
        ResultadoCuenta.ContrasenaInvalida -> "La contraseña debe tener entre 10 y 128 caracteres."
        ResultadoCuenta.SinConsentimiento -> "Acepta la política de privacidad y los términos para continuar."
        ResultadoCuenta.LimiteExcedido -> "Demasiados intentos. Espera un momento y vuelve a intentarlo."
        else -> "No se pudo crear la cuenta. Revisa tu conexión."
    }
}
