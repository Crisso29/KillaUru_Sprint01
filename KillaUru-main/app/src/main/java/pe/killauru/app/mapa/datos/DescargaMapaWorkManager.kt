package pe.killauru.app.mapa.datos

import androidx.work.BackoffPolicy
import androidx.work.Constraints
import androidx.work.ExistingWorkPolicy
import androidx.work.NetworkType
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkInfo
import androidx.work.WorkManager
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.mapa.dominio.EstadoDescarga
import java.util.concurrent.TimeUnit
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-041, FR-043, RNF-15: [DescargaMapa] con un único trabajo de WorkManager. Sin permiso para datos móviles exige una
 * red sin medición (Wi-Fi): así nunca gasta el plan del conductor sin su permiso.
 */
@Singleton
internal class DescargaMapaWorkManager @Inject constructor(private val wm: WorkManager) : DescargaMapa {

    override val estado: Flow<EstadoDescarga> =
        wm.getWorkInfosForUniqueWorkFlow(DescargarMapaWorker.TRABAJO).map { infos -> traducir(infos.firstOrNull()) }

    override fun programar(usarDatosMoviles: Boolean, reemplazar: Boolean) {
        val red = if (usarDatosMoviles) NetworkType.CONNECTED else NetworkType.UNMETERED
        val trabajo = OneTimeWorkRequestBuilder<DescargarMapaWorker>()
            .setConstraints(Constraints.Builder().setRequiredNetworkType(red).build())
            .setBackoffCriteria(BackoffPolicy.EXPONENTIAL, ESPERA_INICIAL_S, TimeUnit.SECONDS)
            .build()
        val politica = if (reemplazar) ExistingWorkPolicy.REPLACE else ExistingWorkPolicy.KEEP
        wm.enqueueUniqueWork(DescargarMapaWorker.TRABAJO, politica, trabajo)
    }

    private fun traducir(info: WorkInfo?): EstadoDescarga = when (info?.state) {
        null, WorkInfo.State.CANCELLED -> EstadoDescarga.SinEmpezar
        WorkInfo.State.ENQUEUED, WorkInfo.State.BLOCKED ->
            if (info.runAttemptCount > 0) EstadoDescarga.Reintentando else EstadoDescarga.Descargando(0)
        WorkInfo.State.RUNNING -> EstadoDescarga.Descargando(
            info.progress.getInt(DescargarMapaWorker.CLAVE_PROGRESO, 0),
            info.progress.getLong(DescargarMapaWorker.CLAVE_BYTES, 0L),
        )
        WorkInfo.State.SUCCEEDED -> info.outputData.getLong(DescargarMapaWorker.CLAVE_ESPERA_BYTES, 0L)
            .let { bytes -> if (bytes > 0) EstadoDescarga.EsperandoWifi(bytes) else EstadoDescarga.Listo }
        WorkInfo.State.FAILED -> EstadoDescarga.Reintentando
    }

    private companion object {
        const val ESPERA_INICIAL_S = 30L
    }
}
