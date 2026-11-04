package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.PerfilUsuario
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.ApiCuenta
import javax.inject.Inject

internal data class PerfilUiState(val perfil: PerfilUsuario? = null, val error: String? = null)

@HiltViewModel
internal class PerfilViewModel @Inject constructor(
    private val repoSesion: RepositorioSesion,
    private val api: ApiCuenta,
) : ViewModel() {

    private val _estado = MutableStateFlow(PerfilUiState())
    val estado: StateFlow<PerfilUiState> = _estado

    init { cargar() }

    private fun cargar() {
        viewModelScope.launch {
            val sesion = repoSesion.obtener() ?: return@launch
            api.obtenerPerfil(sesion.tokenAcceso).onSuccess { perfil ->
                _estado.value = PerfilUiState(perfil = perfil)
            }.onFailure {
                _estado.value = PerfilUiState(error = "No se pudo cargar el perfil.")
            }
        }
    }
}
