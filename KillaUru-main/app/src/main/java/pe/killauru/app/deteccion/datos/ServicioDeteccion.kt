package pe.killauru.app.deteccion.datos

import android.app.Service
import android.content.Intent
import android.os.IBinder
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import pe.killauru.app.deteccion.ui.NotificacionEstado
import pe.killauru.app.deteccion.dominio.IndicadorEstado
import javax.inject.Inject

/** FR-009, R-020: servicio foreground de detección de anomalías (START_STICKY). */
@AndroidEntryPoint
internal class ServicioDeteccion : Service() {

    @Inject lateinit var orquestador: dagger.Lazy<OrquestadorServicio>
    private var iniciado = false

    private val notificacion by lazy { NotificacionEstado(this) }
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Default)

    override fun onCreate() {
        super.onCreate()
        startForeground(
            NotificacionEstado.NOTIF_ID,
            notificacion.construir(IndicadorEstado.SinGps, 0),
        )
        if (!modeloDisponible()) {
            stopSelf()
            return
        }
        orquestador.get().iniciar(scope)
        iniciado = true
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        if (intent?.action == ACCION_DETENER) {
            stopSelf()
        }
        return START_STICKY
    }

    override fun onDestroy() {
        if (iniciado) orquestador.get().detener()
        scope.cancel()
        super.onDestroy()
    }

    override fun onBind(intent: Intent?): IBinder? = null

    private fun modeloDisponible(): Boolean = runCatching {
        val manifiesto = assets.open("modelo/manifiesto.json").bufferedReader()
            .use { org.json.JSONObject(it.readText()) }
        manifiesto.getDouble("umbralPrefiltro")
        assets.open("modelo/" + manifiesto.getString("archivo")).use { it.read() != -1 }
    }.getOrDefault(false)

    companion object {
        const val ACCION_DETENER = "pe.killauru.app.DETENER_DETECCION"
    }
}
