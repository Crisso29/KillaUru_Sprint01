package pe.killauru.app.mapa.datos

import androidx.work.WorkInfo
import pe.killauru.app.mapa.dominio.EstadoDescarga

/**
 * FR-041, FR-043: traduce el trabajo único de WorkManager a lo que ve el conductor. Función pura (se prueba sin
 * Android): en cola después de un intento fallido es «reintentando»; terminado con bytes pendientes, «esperando Wi-Fi».
 */
internal fun estadoDescarga(info: WorkInfo?): EstadoDescarga = when (info?.state) {
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
