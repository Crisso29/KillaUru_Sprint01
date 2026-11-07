package pe.killauru.grabador

import android.content.Context
import androidx.camera.core.CameraSelector
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.video.FileOutputOptions
import androidx.camera.video.Quality
import androidx.camera.video.QualitySelector
import androidx.camera.video.Recorder
import androidx.camera.video.Recording
import androidx.camera.video.VideoCapture
import androidx.camera.video.VideoRecordEvent
import androidx.core.content.ContextCompat
import androidx.lifecycle.LifecycleOwner
import org.json.JSONObject
import java.io.File
import java.util.concurrent.ExecutorService
import java.util.concurrent.Executors

/**
 * T020 / FR-003 — graba video 720p con CameraX; registra la hora del primer cuadro.
 * Solo se activa cuando `esCelularVideo = true` en Metadatos (FR-003).
 */
internal class CamaraGrabacion(
    private val context: Context,
    private val carpeta: File,
    private val onPrimerCuadroNs: (Long) -> Unit,
) {
    private val executor: ExecutorService = Executors.newSingleThreadExecutor()
    private var recording: Recording? = null

    fun iniciar(lifecycleOwner: LifecycleOwner) {
        val future = ProcessCameraProvider.getInstance(context)
        future.addListener({
            val provider = future.get()
            val qualitySelector = QualitySelector.from(Quality.HD)  // 720p
            val recorder = Recorder.Builder().setQualitySelector(qualitySelector).build()
            val videoCapture = VideoCapture.withOutput(recorder)

            provider.bindToLifecycle(lifecycleOwner, CameraSelector.DEFAULT_BACK_CAMERA, videoCapture)

            val archivoVideo = File(carpeta, "video.mp4")
            val outputOptions = FileOutputOptions.Builder(archivoVideo).build()

            recording = videoCapture.output
                .prepareRecording(context, outputOptions)
                .start(ContextCompat.getMainExecutor(context)) { event ->
                    if (event is VideoRecordEvent.Start) {
                        // elapsedRealtimeNanos del primer cuadro (FR-003)
                        val tNs = android.os.SystemClock.elapsedRealtimeNanos()
                        onPrimerCuadroNs(tNs)
                        escribirVideoJson(tNs)
                    }
                }
        }, ContextCompat.getMainExecutor(context))
    }

    fun detener() {
        recording?.stop()
        recording = null
        executor.shutdown()
    }

    private fun escribirVideoJson(tNsPrimerCuadro: Long) {
        val json = JSONObject().apply {
            put("t_ns_primer_cuadro", tNsPrimerCuadro)
            put("fps", 30)
        }
        File(carpeta, "video.json").writeText(json.toString())
    }
}
