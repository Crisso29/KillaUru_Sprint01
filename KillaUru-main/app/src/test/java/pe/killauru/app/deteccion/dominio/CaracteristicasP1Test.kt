package pe.killauru.app.deteccion.dominio

import org.junit.Test
import java.time.Instant
import kotlin.math.abs
import kotlin.test.assertEquals

/**
 * T130 / FR-011, 009 §4: las 20 características del contrato p1.
 * Señal constante → valores analíticamente exactos; señal alternante → cruces > 0.
 * Tolerancia 1e-4 f para aritmética float.
 */
class CaracteristicasP1Test {

    private val eps = 1e-4f

    private fun ventanaConstante(aV: Float, aH: Float, w: Float, kmH: Float): Ventana {
        return Ventana(
            aV = FloatArray(25) { aV },
            aH = FloatArray(25) { aH },
            w = FloatArray(25) { w },
            contextoAv = FloatArray(100) { aV },
            velocidadKmH = kmH,
            inicioUtc = Instant.EPOCH,
        )
    }

    @Test
    fun `vector constante produce caracteristicas verticales correctas`() {
        // Arrange
        val c = CaracteristicasP1.extraer(ventanaConstante(1f, 2f, 0.5f, 60f))

        // Assert — índices verticales según 009 data-model §4
        assertEquals(20, c.size)
        assertEquals(1f, c[0], eps)  // v_media
        assertEquals(0f, c[1], eps)  // v_desv
        assertEquals(1f, c[2], eps)  // v_min
        assertEquals(1f, c[3], eps)  // v_max
        assertEquals(0f, c[4], eps)  // v_pap
        assertEquals(1f, c[5], eps)  // v_rms
        assertEquals(0f, c[6], eps)  // v_curtosis
        assertEquals(0f, c[7], eps)  // v_cruces
        assertEquals(0f, c[8], eps)  // v_jerk
    }

    @Test
    fun `vector constante produce caracteristicas horizontal giroscopio y velocidad correctas`() {
        // Arrange
        val c = CaracteristicasP1.extraer(ventanaConstante(1f, 2f, 0.5f, 60f))

        // Assert — índices h/w/vel según 009 data-model §4
        assertEquals(0f, c[11], eps)   // h_desv
        assertEquals(2f, c[12], eps)   // h_max
        assertEquals(0f, c[13], eps)   // h_pap
        assertEquals(2f, c[14], eps)   // h_rms
        assertEquals(0f, c[15], eps)   // w_desv
        assertEquals(0.5f, c[16], eps) // w_max
        assertEquals(0.5f, c[17], eps) // w_rms
        assertEquals(60f, c[19], eps)  // velocidad_kmh
    }

    @Test
    fun `v_pap es maximo menos minimo de aV`() {
        // Arrange
        val aV = FloatArray(25) { if (it == 0) 10f else if (it == 24) -5f else 0f }
        val ventana = Ventana(aV, FloatArray(25), FloatArray(25), FloatArray(100), 0f, Instant.EPOCH)

        // Act
        val c = CaracteristicasP1.extraer(ventana)

        // Assert — v_pap = 10 - (-5) = 15
        assertEquals(15f, c[4], eps)
    }

    @Test
    fun `v_cruces cuenta cambios de signo estrictos en senal alternante`() {
        // Arrange — señal alternante: v_media≈0, centrada alterna +/-
        val aV = FloatArray(25) { if (it % 2 == 0) 1f else -1f }
        val ventana = Ventana(aV, FloatArray(25), FloatArray(25), FloatArray(100), 0f, Instant.EPOCH)

        // Act
        val c = CaracteristicasP1.extraer(ventana)

        // Assert — 24 pares consecutivos; v_media=0 (13 unos + 12 menos unos = 1/25=0.04)
        // centrada = aV - 0.04; signos alternan siempre → 24 cruces
        assertEquals(24f, c[7], eps)
    }

    @Test
    fun `v_energia_rel es uno cuando la ventana y el contexto tienen la misma senal`() {
        // Arrange — rms(aV) = rms(contexto) → ratio ≈ 1
        val ventana = ventanaConstante(2f, 0f, 0f, 0f)

        // Act
        val c = CaracteristicasP1.extraer(ventana)

        // Assert — v_energia_rel ≈ 1
        assertEquals(1f, c[18], 1e-3f)
    }

    @Test
    fun `velocidad_kmh se copia directamente desde la ventana`() {
        // Arrange
        val ventana = ventanaConstante(0f, 0f, 0f, 72f)

        // Act
        val c = CaracteristicasP1.extraer(ventana)

        // Assert
        assertEquals(72f, c[19], eps)
    }
}
