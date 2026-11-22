package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoRecuperacion
import pe.killauru.app.sesion.dominio.casos.RecuperarContrasenaUseCase
import javax.inject.Inject

internal data class OlvideUiState(val cargando: Boolean = false, val enviado: Boolean = false, val error: String? = null)

/** HU-25 (prototipo «Recuperar contraseña»): el mismo «Listo» exista o no la cuenta, para no revelar quién está registrado. */
@HiltViewModel
internal class OlvideContrasenaViewModel @Inject constructor(
    private val recuperar: RecuperarContrasenaUseCase,
) : ViewModel() {

    private val _estado = MutableStateFlow(OlvideUiState())
    val estado: StateFlow<OlvideUiState> = _estado

    fun enviar(correo: String) {
        viewModelScope.launch {
            _estado.value = OlvideUiState(cargando = true)
            _estado.value = when (recuperar.ejecutar(correo)) {
                ResultadoRecuperacion.Enviado -> OlvideUiState(enviado = true)
                ResultadoRecuperacion.CorreoInvalido -> OlvideUiState(error = "Escribe un correo válido.")
                ResultadoRecuperacion.LimiteExcedido -> OlvideUiState(error = "Demasiados intentos. Espera un momento y vuelve a intentarlo.")
                ResultadoRecuperacion.Error -> OlvideUiState(error = "No se pudo enviar. Revisa tu conexión.")
            }
        }
    }
}
