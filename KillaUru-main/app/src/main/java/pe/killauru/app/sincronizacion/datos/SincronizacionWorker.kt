package pe.killauru.app.sincronizacion.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sincronizacion.dominio.SincronizarEventosUseCase

/** FR-022, FR-029: worker periódico que ejecuta la sincronización de lotes. */
@HiltWorker
internal class SincronizacionWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val sincronizar: SincronizarEventosUseCase,
    private val prefs: RepositorioPreferencias,
    private val vigiaRed: VigiaRed,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result {
        if (!vigiaRed.hayConexion()) return Result.retry()
        val soloMapa = prefs.obtenerSoloVerMapa()
        sincronizar.ejecutar(soloVerMapa = soloMapa, enAhorro = false)
        return Result.success()
    }
}
