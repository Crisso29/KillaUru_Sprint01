package pe.killauru.app.deteccion.datos

import android.content.Context
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager

/** FR-009, FR-010, R-020: lee el acelerómetro a 50 Hz con latencia de 1 s. */
internal class LectorSensores(
    context: Context,
    private val onMuestra: (FloatArray) -> Unit,
) {
    private val sensorManager = context.getSystemService(SensorManager::class.java)
    private val acelerometro = sensorManager.getDefaultSensor(Sensor.TYPE_LINEAR_ACCELERATION)

    private val listener = object : SensorEventListener {
        override fun onSensorChanged(event: SensorEvent) {
            onMuestra(event.values.copyOf())
        }
        override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {}
    }

    fun iniciar() {
        acelerometro?.let {
            sensorManager.registerListener(
                listener, it,
                SensorManager.SENSOR_DELAY_GAME,
                1_000_000,
            )
        }
    }

    fun detener() {
        sensorManager.unregisterListener(listener)
    }
}
