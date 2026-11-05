package pe.killauru.app.sesion.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-066: revoca el token cuando hay red tras un cierre de sesión sin conexión.
 * Se encola como tarea única; si falla, WorkManager lo reintenta.
 */
@HiltWorker
internal class RevocacionPendienteWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val api: IdentidadApi,
) : CoroutineWorker(ctx, params) {

    companion object {
        private const val KEY_TOKEN = "token"

        fun encolar(wm: WorkManager, token: String) {
            val req = OneTimeWorkRequestBuilder<RevocacionPendienteWorker>()
                .setInputData(
                    androidx.work.workDataOf(KEY_TOKEN to token)
                )
                .build()
            wm.enqueue(req)
        }
    }

    override suspend fun doWork(): Result {
        val token = inputData.getString(KEY_TOKEN) ?: return Result.success()
        return try {
            val r = api.cerrarSesion(mapOf("token" to token))
            if (r.isSuccessful) Result.success() else Result.retry()
        } catch (_: Exception) {
            Result.retry()
        }
    }
}
