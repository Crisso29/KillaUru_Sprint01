package pe.killauru.app.deteccion.dominio

import kotlin.math.abs
import kotlin.math.sqrt

/** FR-011, 009 §4: 20 características `float32` del contrato p1. Puerto de `features.py`. */
internal object CaracteristicasP1 {
    private const val HZ = 50f
    private const val EPS = 1e-6f

    fun extraer(v: Ventana): FloatArray {
        val av = v.aV
        val media = av.average().toFloat()
        val desv = desviacion(av)
        val minV = av.min(); val maxV = av.max()
        val minH = v.aH.min(); val maxH = v.aH.max()
        val rmsAv = rms(av)
        return floatArrayOf(
            media, desv, minV, maxV, maxV - minV,
            rmsAv, curtosis(av, desv),
            cruces(av, media), jerk(av),
            abs(minV) / (maxV + EPS), orden(av),
            desviacion(v.aH), maxH, maxH - minH, rms(v.aH),
            desviacion(v.w), v.w.max(), rms(v.w),
            rmsAv / (rms(v.contextoAv) + EPS), v.velocidadKmH,
        )
    }

    private fun rms(a: FloatArray): Float {
        var sum = 0.0
        for (x in a) sum += x.toDouble() * x
        return sqrt(sum / a.size).toFloat()
    }

    private fun desviacion(a: FloatArray): Float {
        val m = a.average()
        var sum = 0.0
        for (x in a) sum += (x - m) * (x - m)
        return sqrt(sum / a.size).toFloat()
    }

    private fun curtosis(a: FloatArray, desv: Float): Float {
        if (desv < EPS) return 0f
        val m = a.average().toFloat()
        var sum = 0.0
        for (x in a) { val z = (x - m) / desv; sum += z * z * z * z }
        return (sum / a.size - 3.0).toFloat()
    }

    private fun cruces(a: FloatArray, media: Float): Float {
        var n = 0
        for (i in 0 until a.size - 1)
            if ((a[i] - media) * (a[i + 1] - media) < 0) n++
        return n.toFloat()
    }

    private fun jerk(a: FloatArray): Float {
        var maxDelta = 0f
        for (i in 0 until a.size - 1) {
            val d = abs(a[i + 1] - a[i])
            if (d > maxDelta) maxDelta = d
        }
        return maxDelta * HZ
    }

    private fun orden(a: FloatArray): Float {
        var iMax = 0; var iMin = 0
        for (i in a.indices) {
            if (a[i] > a[iMax]) iMax = i
            if (a[i] < a[iMin]) iMin = i
        }
        return if (iMax < iMin) 1f else -1f
    }
}
