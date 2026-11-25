package pe.killauru.app.deteccion.datos

import org.junit.Test
import java.time.Instant
import kotlin.test.assertEquals

/** T130 / FR-017: descomposición de ejes = port de gravedad.py + componentes.py. */
class DescomposicionEjesTest {

    private val sut = DescomposicionEjes()

    @Test
    fun `gravedad pura en vertical produce aV y aH cero`() {
        // Arrange — solo gravedad, sin movimiento
        val accel = Array(100) { floatArrayOf(0f, 0f, 9.8f) }
        val gyro = Array(25) { floatArrayOf(0f, 0f, 0f) }

        // Act
        val v = sut.descomponer(accel, gyro, 0f, Instant.EPOCH)

        // Assert — aceleración neta cero en todas las muestras
        v.aV.forEach { assertEquals(0f, it, 1e-3f) }
        v.aH.forEach { assertEquals(0f, it, 1e-3f) }
    }

    @Test
    fun `norma del giroscopio se calcula por pitagoras`() {
        // Arrange — trío 3-4-0 → norma = 5
        val accel = Array(100) { floatArrayOf(0f, 0f, 9.8f) }
        val gyro = Array(25) { floatArrayOf(3f, 4f, 0f) }

        // Act
        val v = sut.descomponer(accel, gyro, 0f, Instant.EPOCH)

        // Assert
        v.w.forEach { assertEquals(5f, it, 1e-3f) }
    }

    @Test
    fun `contextoAv tiene 100 muestras y aV coincide con las ultimas 25`() {
        // Arrange
        val accel = Array(100) { floatArrayOf(0f, 0f, 9.8f) }
        val gyro = Array(25) { floatArrayOf(0f, 0f, 0f) }

        // Act
        val v = sut.descomponer(accel, gyro, 0f, Instant.EPOCH)

        // Assert
        assertEquals(100, v.contextoAv.size)
        assertEquals(25, v.aV.size)
        for (i in 0 until 25)
            assertEquals(v.contextoAv[75 + i], v.aV[i], 1e-5f)
    }

    @Test
    fun `bache vertical genera aV positivo en las ultimas muestras`() {
        // Arrange — primeras 75 = gravedad, últimas 25 = gravedad + 5 m/s²
        val accel = Array(100) { i ->
            if (i < 75) floatArrayOf(0f, 0f, 9.8f) else floatArrayOf(0f, 0f, 14.8f)
        }
        val gyro = Array(25) { floatArrayOf(0f, 0f, 0f) }

        // Act
        val v = sut.descomponer(accel, gyro, 0f, Instant.EPOCH)

        // Assert — las 25 muestras de la ventana deben tener aV > 0
        v.aV.forEach { assert(it > 0f) { "aV debería ser positivo, fue $it" } }
    }

    @Test
    fun `velocidadKmH y inicioUtc se propagan sin cambios`() {
        // Arrange
        val accel = Array(100) { floatArrayOf(0f, 0f, 9.8f) }
        val gyro = Array(25) { floatArrayOf(0f, 0f, 0f) }
        val utc = Instant.ofEpochSecond(1_000)

        // Act
        val v = sut.descomponer(accel, gyro, 72f, utc)

        // Assert
        assertEquals(72f, v.velocidadKmH, 1e-5f)
        assertEquals(utc, v.inicioUtc)
    }
}
