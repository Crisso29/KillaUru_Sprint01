package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.casos.CerrarSesionUseCase
import javax.inject.Inject

@HiltViewModel
internal class CerrarSesionViewModel @Inject constructor(
    private val cerrarSesion: CerrarSesionUseCase,
) : ViewModel() {
    fun cerrarSesion(onCerrada: () -> Unit) {
        viewModelScope.launch { cerrarSesion.ejecutar(); onCerrada() }
    }
}
