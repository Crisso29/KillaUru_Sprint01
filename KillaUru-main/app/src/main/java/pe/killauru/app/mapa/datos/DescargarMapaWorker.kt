package pe.killauru.app.mapa.datos

import android.content.Context
import android.net.ConnectivityManager
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import androidx.work.workDataOf
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import pe.killauru.app.mapa.dominio.ActualizarMapaUseCase
import pe.killauru.app.mapa.dominio.ResultadoMapa

/**
 * FR-041…FR-044: comprueba el manifiesto y descarga el mapa en segundo plano (sigue aunque se cierre la app). Informa
 * el porcentaje solo cuando cambia, para no saturar a WorkManager con cada bloque de 64 KB.
 */
@HiltWorker
internal class DescargarMapaWorker @AssistedInject constructor(
    @Assisted ctx: Context,
    @Assisted params: WorkerParameters,
    private val actualizar: ActualizarMapaUseCase,
) : CoroutineWorker(ctx, params) {

    override suspend fun doWork(): Result {
        val hayWifi = applicationContext.getSystemService(ConnectivityManager::class.java)?.isActiveNetworkMetered == false
        var ultimo = -1
        val resultado = actualizar.ejecutar(hayWifi) { porcentaje, bytes ->
            if (porcentaje != ultimo) {
                ultimo = porcentaje
                setProgressAsync(workDataOf(CLAVE_PROGRESO to porcentaje, CLAVE_BYTES to bytes))
            }
        }
        return when (resultado) {
            is ResultadoMapa.Fallo -> if (runAttemptCount < MAX_INTENTOS) Result.retry() else Result.failure()
            is ResultadoMapa.EsperandoWifi -> Result.success(workDataOf(CLAVE_ESPERA_BYTES to resultado.bytes))
            else -> Result.success()
        }
    }

    companion object {
        const val TRABAJO = "descargar_mapa"
        const val CLAVE_PROGRESO = "progreso"
        const val CLAVE_BYTES = "bytes"
        const val CLAVE_ESPERA_BYTES = "espera_bytes"
        private const val MAX_INTENTOS = 10
    }
}
