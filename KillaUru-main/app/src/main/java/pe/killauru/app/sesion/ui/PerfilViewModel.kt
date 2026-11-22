package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.casos.ObtenerMisDatosUseCase
import javax.inject.Inject

internal data class PerfilUiState(val cargando: Boolean = true, val datos: MisDatos? = null, val error: String? = null)

/** FR-068, HU-24 (prototipo «Mi perfil»): todo lo que KillaUru guarda de la cuenta, pedido al servidor. */
@HiltViewModel
internal class PerfilViewModel @Inject constructor(
    private val obtener: ObtenerMisDatosUseCase,
) : ViewModel() {

    private val _estado = MutableStateFlow(PerfilUiState())
    val estado: StateFlow<PerfilUiState> = _estado

    init { cargar() }

    /** También sirve para «Reintentar» cuando no hubo red. */
    fun cargar() {
        viewModelScope.launch {
            _estado.value = PerfilUiState(cargando = true)
            val datos = obtener.ejecutar()
            _estado.value = if (datos != null) PerfilUiState(cargando = false, datos = datos)
            else PerfilUiState(cargando = false, error = "No se pudieron cargar tus datos. Revisa tu conexión.")
        }
    }
}
