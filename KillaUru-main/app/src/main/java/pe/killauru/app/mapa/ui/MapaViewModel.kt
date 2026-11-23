package pe.killauru.app.mapa.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.filter
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.corredor.dominio.FuenteCorredor
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.EstadoDescarga
import pe.killauru.app.mapa.dominio.FuenteEstiloMapa
import pe.killauru.app.mapa.dominio.RepositorioAnomalias
import javax.inject.Inject

/** Qué dibuja la pestaña Mapa: «Aún no tienes el mapa» o el mapa base local con la vía. */
internal sealed interface EstadoMapaUi {
    data object Cargando : EstadoMapaUi
    data object SinMapa : EstadoMapaUi

    /** [via]: la PE-28A como pares (longitud, latitud), en el orden de MapLibre. */
    data class ConMapa(val estilo: String, val via: List<Pair<Double, Double>>) : EstadoMapaUi
}

/** FR-040, FR-049, T096: el mapa base descargado (sin red), la vía y las anomalías vigentes. */
@HiltViewModel
internal class MapaViewModel @Inject constructor(
    private val repositorio: RepositorioAnomalias,
    private val reloj: Reloj,
    private val estilo: FuenteEstiloMapa,
    private val corredor: FuenteCorredor,
    private val descarga: DescargaMapa,
) : ViewModel() {

    private val _anomalias = MutableStateFlow<List<AnomaliaLocal>>(emptyList())
    val anomalias: StateFlow<List<AnomaliaLocal>> = _anomalias.asStateFlow()

    private val _mapa = MutableStateFlow<EstadoMapaUi>(EstadoMapaUi.Cargando)
    val mapa: StateFlow<EstadoMapaUi> = _mapa.asStateFlow()

    init {
        cargar()
        // Cuando termina una descarga (primera o versión nueva) el mapa cambia sin reabrir la pestaña.
        viewModelScope.launch { descarga.estado.filter { it == EstadoDescarga.Listo }.collect { cargar() } }
    }

    private var carga: Job? = null

    /**
     * También al terminar una descarga: el mapa aparece sin reabrir la app. Cancela la carga anterior: una lectura vieja
     * que termine tarde apuntaría a archivos de la versión ya borrada (carrera vista en el emulador).
     */
    fun cargar() {
        carga?.cancel()
        carga = viewModelScope.launch {
            val ahora = reloj.ahoraConfiable()?.toEpochMilli() ?: 0L
            _anomalias.value = repositorio.vigentes(ahora)
            val json = estilo.estilo()
            _mapa.value = if (json == null) EstadoMapaUi.SinMapa
            else EstadoMapaUi.ConMapa(json, corredor.corredor()?.vertices?.map { it.lon to it.lat }.orEmpty())
        }
    }
}
