package pe.killauru.app

import android.app.Application
import androidx.hilt.work.HiltWorkerFactory
import androidx.work.Configuration
import dagger.hilt.android.HiltAndroidApp
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.launch
import pe.killauru.app.comun.dominio.IniciarApp
import javax.inject.Inject

/**
 * T024, T072: Application con Hilt y WorkManager a demanda (FR-028). Al arrancar ejecuta [IniciarApp] fuera del hilo
 * principal: recupera los envíos cortados, programa la sincronización y la purga, y comprueba el mapa.
 */
@HiltAndroidApp
internal class KillaUruApp : Application(), Configuration.Provider {
    @Inject lateinit var workerFactory: HiltWorkerFactory
    @Inject lateinit var iniciarApp: IniciarApp

    private val alcance = CoroutineScope(SupervisorJob() + Dispatchers.Default)

    override fun onCreate() {
        super.onCreate() // Hilt inyecta aquí
        alcance.launch { iniciarApp.ejecutar() }
    }

    override val workManagerConfiguration: Configuration
        get() = Configuration.Builder()
            .setWorkerFactory(workerFactory)
            .build()
}
