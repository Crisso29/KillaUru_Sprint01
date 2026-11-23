package pe.killauru.app.actualizador.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.EstadoDescarga
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject

/** Lo que muestra la pantalla: el estado de la descarga y el interruptor «Usar datos móviles». */
internal data class PantallaDescargaUi(
    val descarga: EstadoDescarga = EstadoDescarga.SinEmpezar,
    val usarDatosMoviles: Boolean = false,
)

/**
 * FR-041, FR-043, RNF-15 (prototipo «Mapa sin conexión», paso 4, y «Aún no tienes el mapa»): al abrirse pide la
 * descarga (sin cortar una que ya corre); el interruptor guarda la preferencia y reprograma con la nueva red.
 */
@HiltViewModel
internal class DescargaViewModel @Inject constructor(
    private val descarga: DescargaMapa,
    private val prefs: RepositorioPreferencias,
) : ViewModel() {

    private val moviles = MutableStateFlow(false)

    val estado: StateFlow<PantallaDescargaUi> = combine(descarga.estado, moviles, ::PantallaDescargaUi)
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(ESPERA_MS), PantallaDescargaUi())

    init {
        viewModelScope.launch {
            moviles.value = prefs.obtenerUsarDatosMoviles()
            descarga.programar(moviles.value, reemplazar = false)
        }
    }

    /** «Descargar ahora»: reemplaza una descarga que esperaba Wi-Fi o un reintento; sigue del parcial. */
    fun descargarAhora() = descarga.programar(moviles.value, reemplazar = true)

    fun cambiarDatosMoviles(valor: Boolean) {
        moviles.value = valor
        viewModelScope.launch {
            prefs.guardarUsarDatosMoviles(valor)
            descarga.programar(valor, reemplazar = true)
        }
    }

    private companion object {
        const val ESPERA_MS = 5_000L
    }
}
