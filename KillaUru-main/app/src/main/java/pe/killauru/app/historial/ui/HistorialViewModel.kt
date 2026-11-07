package pe.killauru.app.historial.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.historial.dominio.RepositorioRecorridos
import pe.killauru.app.historial.dominio.ViajeRecorrido
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

internal data class HistorialUiState(
    val recorridos: List<ViajeRecorrido> = emptyList(),
    val registrado: Boolean = false,
    val cargando: Boolean = true,
)

@HiltViewModel
internal class HistorialViewModel @Inject constructor(
    private val repositorio: RepositorioRecorridos,
    private val repoSesion: RepositorioSesion,
) : ViewModel() {

    private val _uiState = MutableStateFlow(HistorialUiState())
    val uiState: StateFlow<HistorialUiState> = _uiState

    init {
        cargar()
    }

    private fun cargar() {
        viewModelScope.launch {
            val registrado = repoSesion.obtener() != null
            val recorridos = if (registrado) repositorio.todos() else emptyList()
            _uiState.value = HistorialUiState(
                recorridos = recorridos,
                registrado = registrado,
                cargando = false,
            )
        }
    }
}
