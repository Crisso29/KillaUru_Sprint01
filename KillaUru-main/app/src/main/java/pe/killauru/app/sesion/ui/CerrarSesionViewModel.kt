package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import pe.killauru.app.sesion.dominio.casos.CerrarSesionUseCase
import javax.inject.Inject

/**
 * FR-069 (prototipo «Cerrar sesión»): antes de confirmar, cuenta las detecciones por enviar para decir que no se pierden
 * (se enviarán sin cuenta, RNF-10).
 */
@HiltViewModel
internal class CerrarSesionViewModel @Inject constructor(
    private val cerrarSesion: CerrarSesionUseCase,
    private val eventos: RepositorioEventos,
) : ViewModel() {

    private val _pendientes = MutableStateFlow(0)
    val pendientes: StateFlow<Int> = _pendientes

    init { viewModelScope.launch { _pendientes.value = eventos.contarPendientes() } }

    fun cerrarSesion(onCerrada: () -> Unit) {
        viewModelScope.launch { cerrarSesion.ejecutar(); onCerrada() }
    }
}
