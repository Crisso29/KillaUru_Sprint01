package pe.killauru.app.sesion.datos

import androidx.work.Constraints
import androidx.work.ExistingWorkPolicy
import androidx.work.NetworkType
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import pe.killauru.app.sesion.dominio.RevocacionPendiente
import javax.inject.Inject
import javax.inject.Singleton

/** Implementación de [RevocacionPendiente]: guarda el token cifrado y programa un único trabajo con red. */
@Singleton
internal class RevocacionPendienteWorkManager @Inject constructor(
    private val almacen: AlmacenRevocacion,
    private val wm: WorkManager,
) : RevocacionPendiente {

    override suspend fun programar(tokenRenovacion: String) {
        almacen.guardar(tokenRenovacion)
        val trabajo = OneTimeWorkRequestBuilder<RevocacionPendienteWorker>()
            .setConstraints(Constraints.Builder().setRequiredNetworkType(NetworkType.CONNECTED).build())
            .build()
        // REPLACE: si ya había uno esperando, el nuevo lee el último token guardado.
        wm.enqueueUniqueWork(RevocacionPendienteWorker.TRABAJO, ExistingWorkPolicy.REPLACE, trabajo)
    }
}
