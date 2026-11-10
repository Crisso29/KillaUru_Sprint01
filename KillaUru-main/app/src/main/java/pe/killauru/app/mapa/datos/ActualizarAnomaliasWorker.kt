package pe.killauru.app.mapa.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.mapa.dominio.ActualizarAnomaliasUseCase
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.sesion.dominio.RepositorioSesion

/**
 * FR-046: worker periódico (15 min con red) que delega en [ActualizarAnomaliasUseCase].
 * El use case gestiona todo el estado; este worker solo determina el [Alcance].
 */
@HiltWorker
internal class ActualizarAnomaliasWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val sesion: RepositorioSesion,
    private val actualizar: ActualizarAnomaliasUseCase,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result {
        val alcance = if (sesion.obtenerSesion() != null) Alcance.Registrado else Alcance.Anonimo
        return when (actualizar.actualizar(alcance)) {
            is ActualizarAnomaliasUseCase.Resultado.Exito,
            is ActualizarAnomaliasUseCase.Resultado.SinCambios -> Result.success()
            is ActualizarAnomaliasUseCase.Resultado.Error -> Result.retry()
        }
    }
}
