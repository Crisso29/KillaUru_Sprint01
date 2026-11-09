package pe.killauru.app.sincronizacion.datos

import android.content.Context
import androidx.work.Constraints
import androidx.work.ExistingPeriodicWorkPolicy
import androidx.work.NetworkType
import androidx.work.PeriodicWorkRequestBuilder
import androidx.work.WorkManager
import java.util.concurrent.TimeUnit
import javax.inject.Inject

/** FR-022: programa SincronizacionWorker y PurgaWorker periódicos con WorkManager. */
internal class ProgramadorSync @Inject constructor(@dagger.hilt.android.qualifiers.ApplicationContext private val context: Context) {

    private companion object {
        const val TRABAJO_SYNC = "killauru_sync"
        const val TRABAJO_PURGA = "killauru_purga"
    }

    fun programar() {
        val wm = WorkManager.getInstance(context)
        val restricciones = Constraints.Builder()
            .setRequiredNetworkType(NetworkType.CONNECTED)
            .build()
        val sync = PeriodicWorkRequestBuilder<SincronizacionWorker>(15, TimeUnit.MINUTES)
            .setConstraints(restricciones)
            .build()
        wm.enqueueUniquePeriodicWork(TRABAJO_SYNC, ExistingPeriodicWorkPolicy.KEEP, sync)
        val purga = PeriodicWorkRequestBuilder<PurgaWorker>(1, TimeUnit.DAYS).build()
        wm.enqueueUniquePeriodicWork(TRABAJO_PURGA, ExistingPeriodicWorkPolicy.KEEP, purga)
    }
}
