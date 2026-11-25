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
        const val KEY_TEMPERATURA = "temperatura"
        const val KEY_UMBRAL_PAP = "umbralPicoAPicoMs2"
    }

    override suspend fun doWork(): Result {
        val version = inputData.getString(KEY_VERSION) ?: return Result.failure()
        val archivo = inputData.getString(KEY_ARCHIVO) ?: return Result.failure()
        val temperatura = inputData.getFloat(KEY_TEMPERATURA, 1.0f)
        val umbral = inputData.getFloat(KEY_UMBRAL_PAP, 1.2f)
        val manifiesto = ManifiestoModelo(
            version = version, archivo = archivo,
            clases = listOf("Normal", "Bache", "Hundimiento"),
            versionMinimaApp = "1.0.0",
            temperatura = temperatura,
            umbralPicoAPicoMs2 = umbral,
        )
        return when (actualizar.evaluar(manifiesto)) {
            is ActualizarModeloUseCase.Resultado.Activado -> Result.success()
            is ActualizarModeloUseCase.Resultado.IncompatibleConApp -> Result.failure()
            is ActualizarModeloUseCase.Resultado.SinCambios -> Result.success()
        }
    }
}
