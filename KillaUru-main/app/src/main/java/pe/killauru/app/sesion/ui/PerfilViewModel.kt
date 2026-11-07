package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.PerfilUsuario
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

internal data class PerfilUiState(val perfil: PerfilUsuario? = null, val error: String? = null)

@HiltViewModel
internal class PerfilViewModel @Inject constructor(
    private val repoSesion: RepositorioSesion,
) : ViewModel() {

    private val _estado = MutableStateFlow(PerfilUiState())
    val estado: StateFlow<PerfilUiState> = _estado

    init { cargar() }

    private fun cargar() {
        viewModelScope.launch {
            val sesion = repoSesion.obtener()
            if (sesion != null) {
                _estado.value = PerfilUiState(
                    perfil = PerfilUsuario(sesion.usuarioId, "", null, "", false)
                )
            } else {
                _estado.value = PerfilUiState(error = "No hay sesión activa.")
            }
        }
    }
}
