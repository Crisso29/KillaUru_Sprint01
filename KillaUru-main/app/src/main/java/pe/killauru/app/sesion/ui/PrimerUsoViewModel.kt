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

    /** Términos de esta sesión del flujo: se muestran al pulsar «Comenzar» y al volver desde el vehículo. */
    private var terminos: EstadoPrimerUso.MostrarTerminos? = null

    /** Primera vez = nunca aceptó: solo entonces hay bienvenida a la que volver. */
    private var primeraVez = false

    init {
        viewModelScope.launch { _estado.value = pasoInicial() }
    }

    private suspend fun pasoInicial(): EstadoPrimerUso = when (val r = revisarVersiones.revisar()) {
        is ResultadoVersiones.Vigente -> if (prefs.obtenerCategoria() == null) EstadoPrimerUso.SeleccionarVehiculo
            else EstadoPrimerUso.Completado
        is ResultadoVersiones.PedirAceptacion -> {
            val pendientes = EstadoPrimerUso.MostrarTerminos(r.terminosTexto, r.politicaTexto, r.terminosVersion, r.politicaVersion)
            terminos = pendientes
            primeraVez = prefs.obtenerTerminosVersion() == null
            if (primeraVez) EstadoPrimerUso.Bienvenida else pendientes
        }
    }

    fun comenzar() {
        terminos?.let { _estado.value = it }
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

    fun permisosListos() { _estado.value = EstadoPrimerUso.DescargarMapa }

    /** «Seguir mientras descarga»: la descarga continúa en segundo plano. */
    fun seguirTrasMapa() { _estado.value = EstadoPrimerUso.CuentaONo }

    fun completar() { _estado.value = EstadoPrimerUso.Completado }

    /** «Atrás» del prototipo: un paso hacia el inicio; si no hay paso anterior, se queda (no borra nada guardado). */
    fun volver() {
        _estado.value = when (_estado.value) {
            EstadoPrimerUso.CuentaONo -> EstadoPrimerUso.DescargarMapa
            EstadoPrimerUso.DescargarMapa -> EstadoPrimerUso.SolicitarPermisos
            EstadoPrimerUso.SolicitarPermisos -> EstadoPrimerUso.SeleccionarVehiculo
            EstadoPrimerUso.SeleccionarVehiculo -> terminos ?: return
            is EstadoPrimerUso.MostrarTerminos -> if (primeraVez) EstadoPrimerUso.Bienvenida else return
            else -> return
        }
    }

    /** Hay un paso anterior al que volver (para mostrar o no la flecha). */
    fun puedeVolver(estado: EstadoPrimerUso): Boolean = when (estado) {
        EstadoPrimerUso.CuentaONo, EstadoPrimerUso.DescargarMapa, EstadoPrimerUso.SolicitarPermisos -> true
        EstadoPrimerUso.SeleccionarVehiculo -> terminos != null
        is EstadoPrimerUso.MostrarTerminos -> primeraVez
        else -> false
    }
}
