package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.casos.RegistrarCuentaUseCase
import javax.inject.Inject

internal data class RegistroUiState(val error: String? = null, val cargando: Boolean = false)

@HiltViewModel
internal class RegistroViewModel @Inject constructor(
    private val registrar: RegistrarCuentaUseCase,
) : ViewModel() {

    private val _estado = MutableStateFlow(RegistroUiState())
    val estado: StateFlow<RegistroUiState> = _estado

    fun registrar(correo: String, contrasena: String, onRegistrado: () -> Unit) {
        viewModelScope.launch {
            _estado.value = RegistroUiState(cargando = true)
            when (registrar.ejecutar(correo, contrasena)) {
                ResultadoCuenta.Exito -> { _estado.value = RegistroUiState(); onRegistrado() }
                ResultadoCuenta.Conflicto -> _estado.value = RegistroUiState(error = "El correo ya tiene cuenta.")
                ResultadoCuenta.LimiteAlcanzado -> _estado.value = RegistroUiState(error = "Demasiados intentos. Espera un momento.")
                else -> _estado.value = RegistroUiState(error = "No se pudo crear la cuenta.")
            }
        }
    }
}
