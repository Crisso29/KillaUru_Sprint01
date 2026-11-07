package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject

/** FR-005, FR-007, FR-008: estado de la pantalla de ajustes. */
@HiltViewModel
internal class AjustesViewModel @Inject constructor(
    private val prefs: RepositorioPreferencias,
) : ViewModel() {

    private val _estado = MutableStateFlow(EstadoAjustes())
    val estado: StateFlow<EstadoAjustes> = _estado

    init {
        viewModelScope.launch { cargar() }
    }

    private suspend fun cargar() {
        _estado.value = EstadoAjustes(
            categoria = prefs.obtenerCategoria(),
            soloVerMapa = prefs.obtenerSoloVerMapa(),
            usarDatosMoviles = prefs.obtenerUsarDatosMoviles(),
            mapaVersion = prefs.obtenerMapaVersion(),
            modeloVersion = prefs.obtenerModeloVersion(),
        )
    }

    fun cambiarSoloVerMapa(valor: Boolean) {
        viewModelScope.launch {
            prefs.guardarSoloVerMapa(valor)
            _estado.value = _estado.value.copy(soloVerMapa = valor)
        }
    }

    fun cambiarUsarDatosMoviles(valor: Boolean) {
        viewModelScope.launch {
            prefs.guardarUsarDatosMoviles(valor)
            _estado.value = _estado.value.copy(usarDatosMoviles = valor)
        }
    }

    fun cambiarCategoria(cat: CategoriaVehiculo) {
        viewModelScope.launch {
            prefs.guardarCategoria(cat)
            _estado.value = _estado.value.copy(categoria = cat)
        }
    }
}

internal data class EstadoAjustes(
    val categoria: CategoriaVehiculo? = null,
    val soloVerMapa: Boolean = false,
    val usarDatosMoviles: Boolean = false,
    val mapaVersion: String? = null,
    val modeloVersion: String? = null,
)
