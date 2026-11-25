package pe.killauru.app.deteccion.datos

import android.content.Context
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager

/**
 * FR-009, FR-010, R-020: lee el acelerómetro crudo (TYPE_ACCELEROMETER, incluye gravedad) y el
 * giroscopio a 50 Hz. Emite (accel, giroscopio_último) en cada evento del acelerómetro.
 */
internal class LectorSensores(
    context: Context,
    private val onMuestra: (accel: FloatArray, gyro: FloatArray) -> Unit,
) {
    private val sensorManager = context.getSystemService(SensorManager::class.java)
    private val acelerometro = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)
    private val giroscopio = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE)
    private var ultimoGyro = FloatArray(3)

    private val listener = object : SensorEventListener {
        override fun onSensorChanged(event: SensorEvent) {
            if (event.sensor.type == Sensor.TYPE_GYROSCOPE) {
                ultimoGyro = event.values.copyOf()
            } else {
                onMuestra(event.values.copyOf(), ultimoGyro.copyOf())
            }
        }
        override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {}
    }

    fun iniciar() {
        acelerometro?.let {
            sensorManager.registerListener(listener, it, SensorManager.SENSOR_DELAY_GAME, 1_000_000)
        }
        giroscopio?.let {
            sensorManager.registerListener(listener, it, SensorManager.SENSOR_DELAY_GAME, 1_000_000)
        }
    }

    fun detener() {
        sensorManager.unregisterListener(listener)
    }
}
