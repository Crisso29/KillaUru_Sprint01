package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/** FR-005, FR-007, FR-008: estado de la pantalla de ajustes. */
@HiltViewModel
internal class AjustesViewModel @Inject constructor(
    private val prefs: RepositorioPreferencias,
    private val sesion: RepositorioSesion,
) : ViewModel() {

    private val _estado = MutableStateFlow(EstadoAjustes())
    val estado: StateFlow<EstadoAjustes> = _estado

    init {
        viewModelScope.launch { cargar() }
    }

    /** Al volver a Ajustes (p. ej. tras iniciar o cerrar sesión) se relee todo: la pestaña guarda su ViewModel. */
    fun recargar() {
        viewModelScope.launch { cargar() }
    }

    private suspend fun cargar() {
        _estado.value = EstadoAjustes(
            categoria = prefs.obtenerCategoria(),
            soloVerMapa = prefs.obtenerSoloVerMapa(),
            usarDatosMoviles = prefs.obtenerUsarDatosMoviles(),
            mapaVersion = prefs.obtenerMapaVersion(),
            modeloVersion = prefs.obtenerModeloVersion(),
            terminosVersion = prefs.obtenerTerminosVersion(),
            terminosAceptadoUtcMs = prefs.obtenerTerminosAceptadoUtc(),
            conCuenta = sesion.obtener() != null,
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
    /** Versión y fecha (ms UTC) de los términos aceptados: «Aceptaste la v1.2 el 14 oct 2026» (FR-007). */
    val terminosVersion: String? = null,
    val terminosAceptadoUtcMs: Long? = null,
    /** Con sesión se ofrece «Mi perfil»; sin ella, «Crear cuenta» e «Iniciar sesión» (FR-060). */
    val conCuenta: Boolean = false,
)
