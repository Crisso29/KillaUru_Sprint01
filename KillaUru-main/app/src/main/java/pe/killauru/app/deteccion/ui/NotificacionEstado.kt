package pe.killauru.app.deteccion.ui

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import androidx.core.app.NotificationCompat
import pe.killauru.app.MainActivity
import pe.killauru.app.deteccion.dominio.IndicadorEstado

/** FR-016, R-020: crea y actualiza la notificación del servicio foreground. */
internal class NotificacionEstado(private val context: Context) {

    companion object {
        const val CANAL_ID = "killauru_deteccion"
        const val NOTIF_ID = 1001
    }

    init { crearCanal() }

    private fun crearCanal() {
        val canal = NotificationChannel(
            CANAL_ID, "Detección KillaUru",
            NotificationManager.IMPORTANCE_LOW,
        ).apply { description = "Estado de la detección de anomalías" }
        context.getSystemService(NotificationManager::class.java)
            .createNotificationChannel(canal)
    }

    fun construir(indicador: IndicadorEstado, pendientes: Int): Notification {
        val texto = textoEstado(indicador, pendientes)
        val intent = PendingIntent.getActivity(
            context, 0,
            Intent(context, MainActivity::class.java),
            PendingIntent.FLAG_IMMUTABLE,
        )
        return NotificationCompat.Builder(context, CANAL_ID)
            .setSmallIcon(android.R.drawable.ic_menu_mylocation)
            .setContentTitle("KillaUru")
            .setContentText(texto)
            .setContentIntent(intent)
            .setOngoing(true)
            .build()
    }
    private fun textoEstado(indicador: IndicadorEstado, pendientes: Int): String {
        return when (indicador) {
            IndicadorEstado.Detectando -> if (pendientes > 0) "$pendientes por enviar" else "Detectando…"
            IndicadorEstado.EnPausa -> "En pausa"
            IndicadorEstado.SinGps -> "Sin GPS"
            IndicadorEstado.ModoAhorro -> "Ahorro de batería"
            IndicadorEstado.SoloMapa -> "Solo ver el mapa"
            IndicadorEstado.SinPermiso -> "Sin permiso de ubicación"
            IndicadorEstado.SinServicio -> "Servicio detenido"
        }
    }
}
