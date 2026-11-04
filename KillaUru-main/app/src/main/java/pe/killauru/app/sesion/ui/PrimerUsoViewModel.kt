package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.ResultadoVersiones
import pe.killauru.app.sesion.dominio.casos.AceptarVersionesUseCase
import pe.killauru.app.sesion.dominio.casos.RevisarVersionesLegalesUseCase
import javax.inject.Inject

/** FR-001, FR-003, FR-004, FR-006, FR-008: orquesta el flujo del primer uso. */
@HiltViewModel
internal class PrimerUsoViewModel @Inject constructor(
    private val revisarVersiones: RevisarVersionesLegalesUseCase,
    private val aceptarVersiones: AceptarVersionesUseCase,
    private val prefs: RepositorioPreferencias,
) : ViewModel() {

    private val _estado = MutableStateFlow<EstadoPrimerUso>(EstadoPrimerUso.Cargando)
    val estado: StateFlow<EstadoPrimerUso> = _estado

    init {
        viewModelScope.launch { revisar() }
    }

    private suspend fun revisar() {
        _estado.value = when (val r = revisarVersiones.revisar()) {
            is ResultadoVersiones.Vigente -> EstadoPrimerUso.Completado
            is ResultadoVersiones.PedirAceptacion -> EstadoPrimerUso.MostrarTerminos(
                r.terminosTexto, r.politicaTexto, r.terminosVersion, r.politicaVersion,
            )
        }
    }

    fun aceptarTerminos(tv: String, pv: String) {
        viewModelScope.launch {
            aceptarVersiones.aceptar(tv, pv)
            _estado.value = EstadoPrimerUso.SeleccionarVehiculo
        }
    }

    fun seleccionarVehiculo(cat: CategoriaVehiculo) {
        viewModelScope.launch {
            prefs.guardarCategoria(cat)
            _estado.value = EstadoPrimerUso.SolicitarPermisos
        }
    }

    fun permisosListos() { _estado.value = EstadoPrimerUso.CuentaONo }
    fun completar() { _estado.value = EstadoPrimerUso.Completado }
}

internal sealed class EstadoPrimerUso {
    object Cargando : EstadoPrimerUso()
    data class MostrarTerminos(
        val terminosTexto: String,
        val politicaTexto: String,
        val terminosVersion: String,
        val politicaVersion: String,
    ) : EstadoPrimerUso()
    object SeleccionarVehiculo : EstadoPrimerUso()
    object SolicitarPermisos : EstadoPrimerUso()
    object CuentaONo : EstadoPrimerUso()
    object Completado : EstadoPrimerUso()
}
