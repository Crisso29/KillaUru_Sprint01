package pe.killauru.app.mapa.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.mapa.dominio.ActualizarMapaUseCase

/** FR-043, FR-044: worker que descarga el archivo PMTiles con reanudación. */
@HiltWorker
internal class DescargarMapaWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val cdnApi: CdnApi,
    private val descarga: DescargaReanudable,
    private val actualizar: ActualizarMapaUseCase,
) : CoroutineWorker(ctx, params) {

    companion object {
        const val KEY_URL = "url"
        const val KEY_NOMBRE = "nombre"
        const val KEY_SHA256 = "sha256"
        const val KEY_VERSION = "version"
    }

    override suspend fun doWork(): Result {
        val url = inputData.getString(KEY_URL) ?: return Result.failure()
        val nombre = inputData.getString(KEY_NOMBRE) ?: return Result.failure()
        val sha256 = inputData.getString(KEY_SHA256) ?: return Result.failure()
        val version = inputData.getString(KEY_VERSION) ?: return Result.failure()
        return when (descarga.descargar(url, nombre, sha256)) {
            is DescargaReanudable.Resultado.Exitosa -> {
                actualizar.marcarDescargado(version)
                Result.success()
            }
            is DescargaReanudable.Resultado.Fallida -> Result.retry()
        }
    }
}
