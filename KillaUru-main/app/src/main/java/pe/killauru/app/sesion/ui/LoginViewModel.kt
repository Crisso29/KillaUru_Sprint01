package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.casos.IniciarSesionUseCase
import javax.inject.Inject

internal data class LoginUiState(val error: String? = null, val cargando: Boolean = false)

@HiltViewModel
internal class LoginViewModel @Inject constructor(
    private val iniciar: IniciarSesionUseCase,
) : ViewModel() {

    private val _estado = MutableStateFlow(LoginUiState())
    val estado: StateFlow<LoginUiState> = _estado

    fun iniciarSesion(correo: String, contrasena: String, onExito: () -> Unit) {
        viewModelScope.launch {
            _estado.value = LoginUiState(cargando = true)
            val resultado = iniciar.ejecutar(correo, contrasena)
            if (resultado is ResultadoCuenta.Exitoso) {
                _estado.value = LoginUiState()
                onExito()
            } else {
                _estado.value = LoginUiState(error = mensaje(resultado))
            }
        }
    }

    /** FR-061: ante 401 siempre el mismo mensaje (RN-19), exista o no la cuenta; R-033 para administración. */
    private fun mensaje(resultado: ResultadoCuenta): String = when (resultado) {
        ResultadoCuenta.CredencialesInvalidas -> "Correo o contraseña incorrectos."
        ResultadoCuenta.CuentaDeAdministracion -> "Esta cuenta es de administración; usa el panel."
        ResultadoCuenta.LimiteExcedido -> "Demasiados intentos. Espera un momento y vuelve a intentarlo."
        else -> "No se pudo iniciar sesión. Revisa tu conexión."
    }
}
