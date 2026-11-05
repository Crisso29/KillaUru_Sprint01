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

    @Inject lateinit var orquestador: OrquestadorServicio

    private val notificacion by lazy { NotificacionEstado(this) }
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Default)

    override fun onCreate() {
        super.onCreate()
        startForeground(
            NotificacionEstado.NOTIF_ID,
            notificacion.construir(IndicadorEstado.SinGps, 0),
        )
        orquestador.iniciar(scope)
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        if (intent?.action == ACCION_DETENER) {
            stopSelf()
        }
        return START_STICKY
    }

    override fun onDestroy() {
        orquestador.detener()
        scope.cancel()
        super.onDestroy()
    }

    override fun onBind(intent: Intent?): IBinder? = null

    companion object {
        const val ACCION_DETENER = "pe.killauru.app.DETENER_DETECCION"
    }
}
