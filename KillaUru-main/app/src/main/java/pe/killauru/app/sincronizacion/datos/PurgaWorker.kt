package pe.killauru.app.sincronizacion.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.sincronizacion.dominio.PurgarLocalUseCase
import java.time.Duration

/** FR-038: elimina eventos en estados finales mayores a 7 días. */
@HiltWorker
internal class PurgaWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val purgar: PurgarLocalUseCase,
    private val reloj: Reloj,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result {
        val ahora = reloj.ahoraConfiable() ?: return Result.success()
        purgar.ejecutar(ahora.minus(Duration.ofDays(7)).toEpochMilli())
        return Result.success()
    }
}
