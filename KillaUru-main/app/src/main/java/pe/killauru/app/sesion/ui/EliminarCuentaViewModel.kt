package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import pe.killauru.app.sesion.dominio.casos.EliminarCuentaUseCase
import javax.inject.Inject

internal data class EliminarUiState(val cargando: Boolean = false, val error: String? = null)

/** FR-067, HU-23 (prototipo «Eliminar mi cuenta»): pide la contraseña; si es incorrecta lo dice y no sale. */
@HiltViewModel
internal class EliminarCuentaViewModel @Inject constructor(
    private val eliminar: EliminarCuentaUseCase,
) : ViewModel() {

    private val _estado = MutableStateFlow(EliminarUiState())
    val estado: StateFlow<EliminarUiState> = _estado

    fun eliminar(contrasena: String, onEliminada: () -> Unit) {
        viewModelScope.launch {
            _estado.value = EliminarUiState(cargando = true)
            when (eliminar.ejecutar(contrasena)) {
                is ResultadoEliminacion.Eliminada -> { _estado.value = EliminarUiState(); onEliminada() }
                is ResultadoEliminacion.ConfirmacionInvalida -> _estado.value = EliminarUiState(error = "La contraseña no es correcta.")
                is ResultadoEliminacion.Error -> _estado.value = EliminarUiState(error = "No se pudo eliminar la cuenta. Revisa tu conexión.")
            }
        }
    }
}
