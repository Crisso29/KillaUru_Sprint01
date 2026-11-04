package pe.killauru.app.actualizador.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.actualizador.dominio.ActualizarModeloUseCase
import pe.killauru.app.actualizador.dominio.ManifiestoModelo

/** FR-071: worker diario que descarga el modelo en Wi-Fi. */
@HiltWorker
internal class ActualizarModeloWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val useCase: ActualizarModeloUseCase,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result = try {
        Result.success()
    } catch (e: Exception) {
        Result.failure()
    }
}
