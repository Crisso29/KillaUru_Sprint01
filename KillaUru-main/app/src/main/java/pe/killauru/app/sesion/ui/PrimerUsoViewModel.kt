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

/**
 * FR-001, FR-004, FR-006, R-024: máquina de estados del primer uso. Decide el primer paso con lo que ya está guardado:
 * la bienvenida solo para quien nunca aceptó; una versión nueva de los términos se pide sola, sin repetir vehículo ni
 * permisos; y si falta el vehículo (la app se cerró a mitad), retoma ahí porque sin categoría no se sabe si detectar.
 */
@HiltViewModel
internal class PrimerUsoViewModel @Inject constructor(
    private val revisarVersiones: RevisarVersionesLegalesUseCase,
    private val aceptarVersiones: AceptarVersionesUseCase,
    private val prefs: RepositorioPreferencias,
) : ViewModel() {

    private val _estado = MutableStateFlow<EstadoPrimerUso>(EstadoPrimerUso.Cargando)
    val estado: StateFlow<EstadoPrimerUso> = _estado

    /** Términos a mostrar cuando el usuario pulse «Comenzar» en la bienvenida. */
    private var terminosPendientes: EstadoPrimerUso.MostrarTerminos? = null

    init {
        viewModelScope.launch { _estado.value = pasoInicial() }
    }

    private suspend fun pasoInicial(): EstadoPrimerUso = when (val r = revisarVersiones.revisar()) {
        is ResultadoVersiones.Vigente -> if (prefs.obtenerCategoria() == null) EstadoPrimerUso.SeleccionarVehiculo
            else EstadoPrimerUso.Completado
        is ResultadoVersiones.PedirAceptacion -> {
            val terminos = EstadoPrimerUso.MostrarTerminos(r.terminosTexto, r.politicaTexto, r.terminosVersion, r.politicaVersion)
            if (prefs.obtenerTerminosVersion() == null) EstadoPrimerUso.Bienvenida.also { terminosPendientes = terminos }
            else terminos
        }
    }

    fun comenzar() {
        terminosPendientes?.let { _estado.value = it }
    }

    fun aceptarTerminos(terminosVersion: String, politicaVersion: String) {
        viewModelScope.launch {
            aceptarVersiones.aceptar(terminosVersion, politicaVersion)
            _estado.value = if (prefs.obtenerCategoria() == null) EstadoPrimerUso.SeleccionarVehiculo
                else EstadoPrimerUso.Completado
        }
    }

    fun seleccionarVehiculo(categoria: CategoriaVehiculo) {
        viewModelScope.launch {
            prefs.guardarCategoria(categoria)
            _estado.value = EstadoPrimerUso.SolicitarPermisos
        }
    }

    fun permisosListos() { _estado.value = EstadoPrimerUso.CuentaONo }

    fun completar() { _estado.value = EstadoPrimerUso.Completado }
}
