package pe.killauru.app.mapa.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.mapa.dominio.ActualizarAnomaliasUseCase

/** FR-040: worker que actualiza el delta de anomalías desde la API. */
@HiltWorker
internal class ActualizarAnomaliasWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val api: MapaApi,
    private val mapeador: MapeadorGeoJson,
    private val actualizar: ActualizarAnomaliasUseCase,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result = try {
        val resp = api.anomalias()
        if (resp.isSuccessful) {
            val altas = resp.body()?.let { MapeadorGeoJson.mapear(it) } ?: emptyList()
            actualizar.actualizar(altas, emptyList())
        }
        Result.success()
    } catch (e: Exception) {
        Result.retry()
    }
}
