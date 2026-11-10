package pe.killauru.app.actualizador.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.work.WorkInfo
import androidx.work.WorkManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import javax.inject.Inject

/**
 * FR-041, FR-008: estado de la descarga del mapa base (PMTiles).
 * Solo expone lo necesario para la pantalla; el Worker gestiona la descarga real.
 */
@HiltViewModel
internal class DescargaViewModel @Inject constructor(
    private val workManager: WorkManager,
) : ViewModel() {

    sealed class Estado {
        object Esperando : Estado()
        /** Descarga en curso con [progreso] 0–100 %. */
        data class Descargando(val progreso: Int) : Estado()
        object Completado : Estado()
        object Fallido : Estado()
    }

    val estado = workManager
        .getWorkInfosForUniqueWorkFlow(ETIQUETA_DESCARGA)
        .map { infos ->
            val info = infos.firstOrNull()
            when (info?.state) {
                WorkInfo.State.RUNNING -> {
                    val pct = info.progress.getInt(CLAVE_PROGRESO, 0)
                    Estado.Descargando(pct)
                }
                WorkInfo.State.SUCCEEDED -> Estado.Completado
                WorkInfo.State.FAILED -> Estado.Fallido
                else -> Estado.Esperando
            }
        }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), Estado.Esperando)

    companion object {
        const val ETIQUETA_DESCARGA = "descargar_mapa"
        const val CLAVE_PROGRESO = "progreso"
    }
}
