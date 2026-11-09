package pe.killauru.grabador

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.Service
import android.content.Intent
import android.os.IBinder
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * T018 / FR-001, RF-01 — servicio en primer plano que coordina sensores, GPS y escritura CSV.
 * Arranca y para solo con el vehículo detenido (R-002, R-003); ver MainActivity.
 */
internal class ServicioGrabacion : Service() {

    private var sensores: SensoresGrabacion? = null
    private var gps: GpsGrabacion? = null
    private var accelWriter: EscritorCsv? = null
    private var giroWriter: EscritorCsv? = null
    private var gpsWriter: EscritorCsv? = null
    private var marcasWriter: EscritorCsv? = null
    private var carpetaGrabacion: File? = null

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        iniciarNotificacion()
        val carpeta = crearCarpeta()
        carpetaGrabacion = carpeta

        accelWriter = EscritorCsv(File(carpeta, "acelerometro.csv"), EscritorCsv.Tipo.ACELEROMETRO)
        giroWriter = EscritorCsv(File(carpeta, "giroscopio.csv"), EscritorCsv.Tipo.GIROSCOPIO)
        gpsWriter = EscritorCsv(File(carpeta, "gps.csv"), EscritorCsv.Tipo.GPS)
        marcasWriter = EscritorCsv(File(carpeta, "marcas.csv"), EscritorCsv.Tipo.MARCAS)

        sensores = SensoresGrabacion(this,
            onAccel = { t, x, y, z -> accelWriter?.escribir(t, floatArrayOf(x, y, z)) },
            onGiro  = { t, x, y, z -> giroWriter?.escribir(t, floatArrayOf(x, y, z)) },
        ).also { it.iniciar() }

        gps = GpsGrabacion(this) { t, utc, lat, lon, prec, vel, rumbo ->
            gpsWriter?.escribirGps(t, utc, lat, lon, prec, vel, rumbo)
        }.also { it.iniciar() }

        return START_STICKY
    }

    fun registrarMarca() {
        marcasWriter?.escribirMarca(System.nanoTime(), "copiloto")
    }

    override fun onDestroy() {
        sensores?.detener()
        gps?.detener()
        accelWriter?.close(); giroWriter?.close()
        gpsWriter?.close(); marcasWriter?.close()
        super.onDestroy()
    }

    override fun onBind(intent: Intent?): IBinder? = null

    private fun crearCarpeta(): File {
        val fecha = SimpleDateFormat("yyyyMMdd", Locale.US).format(Date())
        val nombre = "${fecha}-R00-C0"  // recorrido y celular se rellenan desde MainActivity
        val carpeta = File(getExternalFilesDir(null), nombre)
        carpeta.mkdirs()
        return carpeta
    }

    private fun iniciarNotificacion() {
        val canal = NotificationChannel("grabacion", "Grabación activa", NotificationManager.IMPORTANCE_LOW)
        (getSystemService(NOTIFICATION_SERVICE) as NotificationManager).createNotificationChannel(canal)
        val notif = Notification.Builder(this, "grabacion")
            .setContentTitle("KillaUru Grabador")
            .setContentText("Grabando recorrido…")
            .setSmallIcon(android.R.drawable.ic_media_play)
            .build()
        startForeground(1, notif)
    }
}
