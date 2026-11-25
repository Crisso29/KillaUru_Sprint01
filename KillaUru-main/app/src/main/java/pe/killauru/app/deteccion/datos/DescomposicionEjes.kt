package pe.killauru.app.deteccion.datos

import java.time.Instant
import kotlin.math.sqrt
import pe.killauru.app.deteccion.dominio.Ventana

/** FR-017: port de `gravedad.py` + `componentes.py` — estima gravedad y descompone ejes. */
internal class DescomposicionEjes {

    fun descomponer(
        contextoAccel: Array<FloatArray>,
        ventanaGyro: Array<FloatArray>,
        velocidadKmH: Float,
        inicioUtc: Instant,
    ): Ventana {
        val g = gravedad(contextoAccel)
        val gNorm = norma3(g)
        val gHat = if (gNorm < 1e-6f) floatArrayOf(0f, 0f, 1f) else
            floatArrayOf(g[0] / gNorm, g[1] / gNorm, g[2] / gNorm)
        val efectiva = if (gNorm < 1e-6f) 1f else gNorm
        val ctxAv = FloatArray(contextoAccel.size) { i -> punto(contextoAccel[i], gHat) - efectiva }
        val ini = contextoAccel.size - ventanaGyro.size
        val aV = ctxAv.copyOfRange(ini, contextoAccel.size)
        val aH = FloatArray(ventanaGyro.size) { i -> magnitudH(contextoAccel[ini + i], gHat) }
        val w = FloatArray(ventanaGyro.size) { i -> norma3(ventanaGyro[i]) }
        return Ventana(aV, aH, w, ctxAv, velocidadKmH, inicioUtc)
    }

    private fun gravedad(accel: Array<FloatArray>): FloatArray {
        val g = FloatArray(3)
        for (a in accel) { g[0] += a[0]; g[1] += a[1]; g[2] += a[2] }
        val n = accel.size.toFloat()
        return floatArrayOf(g[0] / n, g[1] / n, g[2] / n)
    }

    private fun norma3(v: FloatArray): Float = sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2])

    private fun punto(a: FloatArray, b: FloatArray): Float = a[0] * b[0] + a[1] * b[1] + a[2] * b[2]

    private fun magnitudH(a: FloatArray, gHat: FloatArray): Float {
        val p = punto(a, gHat)
        val hx = a[0] - p * gHat[0]
        val hy = a[1] - p * gHat[1]
        val hz = a[2] - p * gHat[2]
        return sqrt(hx * hx + hy * hy + hz * hz)
    }
}
