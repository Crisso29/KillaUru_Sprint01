package pe.killauru.grabador

import android.content.Context
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager

/**
 * T018 / FR-001, RF-01, diseño-interno §7.4 — captura acelerómetro y giroscopio.
 * 50 Hz pedidos (samplingPeriodUs=20000), entrega en lote ~1 s (maxReportLatencyUs=1_000_000).
 */
internal class SensoresGrabacion(
    context: Context,
    private val onAccel: (tNs: Long, x: Float, y: Float, z: Float) -> Unit,
    private val onGiro: (tNs: Long, x: Float, y: Float, z: Float) -> Unit,
) : SensorEventListener {

    private val sensorManager = context.getSystemService(Context.SENSOR_SERVICE) as SensorManager
    private val acelerometro = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)
    private val giroscopio = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE)

    fun iniciar() {
        // samplingPeriodUs=20000 µs (50 Hz); maxReportLatencyUs=1_000_000 µs (~1 s en lote)
        sensorManager.registerListener(this, acelerometro, 20_000, 1_000_000)
        sensorManager.registerListener(this, giroscopio, 20_000, 1_000_000)
    }

    fun detener() {
        sensorManager.unregisterListener(this)
    }

    override fun onSensorChanged(event: SensorEvent) {
        when (event.sensor.type) {
            Sensor.TYPE_ACCELEROMETER -> onAccel(event.timestamp, event.values[0], event.values[1], event.values[2])
            Sensor.TYPE_GYROSCOPE -> onGiro(event.timestamp, event.values[0], event.values[1], event.values[2])
        }
    }

    override fun onAccuracyChanged(sensor: Sensor, accuracy: Int) = Unit
}
