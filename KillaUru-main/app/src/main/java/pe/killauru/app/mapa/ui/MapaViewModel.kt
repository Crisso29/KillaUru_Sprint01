package pe.killauru.app.mapa.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.mapa.dominio.RepositorioAnomalias
import javax.inject.Inject

/** FR-040, FR-049: VM del mapa con lista de anomalías vigentes. */
@HiltViewModel
internal class MapaViewModel @Inject constructor(
    private val repositorio: RepositorioAnomalias,
    private val reloj: Reloj,
) : ViewModel() {

    private val _anomalias = MutableStateFlow<List<AnomaliaLocal>>(emptyList())
    val anomalias: StateFlow<List<AnomaliaLocal>> = _anomalias.asStateFlow()

    init { cargar() }

    fun cargar() {
        viewModelScope.launch {
            val ahora = reloj.ahoraConfiable()?.toEpochMilli() ?: 0L
            _anomalias.value = repositorio.vigentes(ahora)
        }
    }
}
