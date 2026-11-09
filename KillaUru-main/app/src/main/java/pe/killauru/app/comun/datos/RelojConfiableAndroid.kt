package pe.killauru.app.comun.datos

import java.time.Duration
import java.time.Instant
import javax.inject.Inject
import javax.inject.Singleton
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import pe.killauru.app.comun.dominio.AnclaTiempo
import pe.killauru.app.comun.dominio.Reloj

/**
 * FR-026, R-003: combina un ancla GPS o del servidor con el reloj monotónico para
 * producir una hora UTC confiable sin depender del reloj de pared del dispositivo.
 */
@Singleton
internal class RelojConfiableAndroid @Inject constructor(
    private val dataStore: AnclaTiempoDataStore,
    @dagger.hilt.android.qualifiers.ApplicationContext context: android.content.Context,
) : Reloj {
    private val mutex = Mutex()
    private var anclaEnMemoria: AnclaTiempo? = null
    private val bootCount: Int = RelojSistema.bootCount(context.contentResolver)

    override fun ahoraConfiable(): Instant? {
        val ancla = anclaEnMemoria ?: return null
        return ancla.hora(RelojSistema.elapsed(), bootCount)
    }

    override fun monotonico(): Duration = RelojSistema.elapsed()

    override suspend fun anclarGps(utc: Instant, elapsed: Duration) {
        val ancla = AnclaTiempo(utc, elapsed, bootCount)
        mutex.withLock { anclaEnMemoria = ancla }
        dataStore.guardar(utc, elapsed, bootCount, "gps")
    }

    override suspend fun anclarServidor(utc: Instant) {
        val elapsed = RelojSistema.elapsed()
        val ancla = AnclaTiempo(utc, elapsed, bootCount)
        mutex.withLock { anclaEnMemoria = ancla }
        dataStore.guardar(utc, elapsed, bootCount, "servidor")
    }

    suspend fun restaurarDesdeDataStore() {
        val ancla = dataStore.leer() ?: return
        if (ancla.arranque == bootCount) {
            mutex.withLock { anclaEnMemoria = ancla }
        }
    }
}
