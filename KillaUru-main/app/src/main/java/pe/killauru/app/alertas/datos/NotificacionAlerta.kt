package pe.killauru.app.alertas.datos

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import androidx.core.app.NotificationCompat
import pe.killauru.app.alertas.dominio.Alerta
import javax.inject.Inject

/** FR-058: notificación del sistema para alertas de anomalía. */
internal class NotificacionAlerta @Inject constructor(private val context: Context) {

    companion object {
        const val CANAL_ID = "killauru_alertas"
        const val NOTIF_ID_BASE = 2000
    }

    init { crearCanal() }

    private fun crearCanal() {
        val canal = NotificationChannel(
            CANAL_ID, "Alertas de vía",
            NotificationManager.IMPORTANCE_HIGH,
        ).apply { description = "Avisos de anomalías próximas en la carretera" }
        context.getSystemService(NotificationManager::class.java)
            .createNotificationChannel(canal)
    }

    fun construir(alerta: Alerta): Notification {
        val texto = "${alerta.anomalia.tipo.texto} a ${alerta.distanciaM.toInt()} m"
        return NotificationCompat.Builder(context, CANAL_ID)
            .setSmallIcon(android.R.drawable.ic_dialog_alert)
            .setContentTitle("Anomalía próxima")
            .setContentText(texto)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
            .setAutoCancel(true)
            .build()
    }
}
