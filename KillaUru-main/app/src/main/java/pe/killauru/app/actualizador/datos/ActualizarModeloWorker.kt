package pe.killauru.app.actualizador.datos

import android.content.Context
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.actualizador.dominio.ActualizarModeloUseCase
import pe.killauru.app.actualizador.dominio.ManifiestoModelo

/** FR-070: worker que descarga el manifiesto y activa el nuevo modelo si es compatible. */
@HiltWorker
internal class ActualizarModeloWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val actualizar: ActualizarModeloUseCase,
) : CoroutineWorker(ctx, params) {

    companion object {
        const val KEY_VERSION = "version"
        const val KEY_ARCHIVO = "archivo"
        const val KEY_ESQUEMA = "esquema"
    }

    override suspend fun doWork(): Result {
        val version = inputData.getString(KEY_VERSION) ?: return Result.failure()
        val archivo = inputData.getString(KEY_ARCHIVO) ?: return Result.failure()
        val esquema = inputData.getInt(KEY_ESQUEMA, -1).takeIf { it != -1 } ?: return Result.failure()
        val manifiesto = ManifiestoModelo(
            version = version, archivo = archivo, esquema = esquema,
            etiquetas = listOf("Normal", "Bache", "Hundimiento"),
            severidades = 5, versionMinimaApp = "1.0.0",
        )
        return when (actualizar.evaluar(manifiesto)) {
            is ActualizarModeloUseCase.Resultado.Activado -> Result.success()
            is ActualizarModeloUseCase.Resultado.IncompatibleConApp -> Result.failure()
            is ActualizarModeloUseCase.Resultado.SinCambios -> Result.success()
        }
    }
}
