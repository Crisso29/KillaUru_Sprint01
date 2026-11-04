package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.casos.LoginUseCase
import javax.inject.Inject

internal data class LoginUiState(val error: String? = null, val cargando: Boolean = false)

@HiltViewModel
internal class LoginViewModel @Inject constructor(
    private val login: LoginUseCase,
) : ViewModel() {

    private val _estado = MutableStateFlow(LoginUiState())
    val estado: StateFlow<LoginUiState> = _estado

    fun iniciarSesion(correo: String, contrasena: String, onExito: () -> Unit) {
        viewModelScope.launch {
            _estado.value = LoginUiState(cargando = true)
            when (login.ejecutar(correo, contrasena)) {
                is ResultadoCuenta.Exitoso -> { _estado.value = LoginUiState(); onExito() }
                is ResultadoCuenta.CredencialesInvalidas -> _estado.value = LoginUiState(
                    error = "Esta cuenta no puede acceder desde la app.")
                else -> _estado.value = LoginUiState(error = "Correo o contraseña incorrectos.")
            }
        }
    }
}
