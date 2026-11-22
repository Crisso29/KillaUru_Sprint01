package pe.killauru.app.sesion.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject

/**
 * FR-066, R-023: revoca en el servidor una sesión que se cerró sin red. Corre solo con red; si falla, WorkManager lo
 * reintenta. La lógica está en [EnviarRevocacionPendiente], que se prueba sin Android.
 */
@HiltWorker
internal class RevocacionPendienteWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val envio: EnviarRevocacionPendiente,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result = if (envio.ejecutar()) Result.success() else Result.retry()

    companion object {
        const val TRABAJO = "revocacion_pendiente"
    }
}
