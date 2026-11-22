package pe.killauru.app.historial.ui

import kotlin.math.abs
import kotlin.math.roundToInt

/**
 * Textos de un viaje para «Mis viajes» (prototipo): el tramo por sus pueblos y la duración. Los pueblos y su km son los
 * del corredor PE-28A (los mismos que la franja del panel). Reglas puras: no leen el reloj ni la base.
 */
internal object TextosViaje {
    private val PUEBLOS = listOf(0.0 to "Pisco", 140.0 to "Huaytará", 196.0 to "Rumichaca", 268.0 to "Vinchos", 333.0 to "Ayacucho")
    private const val CERCA_KM = 15.0

    /** El pueblo más cercano si está a menos de 15 km; si no, «km 90». */
    fun lugar(km: Double): String {
        val (kmPueblo, nombre) = PUEBLOS.minBy { abs(it.first - km) }
        return if (abs(kmPueblo - km) <= CERCA_KM) nombre else "km ${km.roundToInt()}"
    }

    /** «1 h 20 min» o «45 min». */
    fun duracion(inicioMs: Long, finMs: Long): String {
        val minutos = ((finMs - inicioMs) / 60_000L).coerceAtLeast(0)
        val (h, m) = minutos / 60 to minutos % 60
        return if (h > 0) "$h h $m min" else "$m min"
    }

    /** «km 333 → 268 · 1 h 20 min». */
    fun detalle(kmDesde: Double, kmHasta: Double, inicioMs: Long, finMs: Long): String =
        "km ${kmDesde.roundToInt()} → ${kmHasta.roundToInt()} · ${duracion(inicioMs, finMs)}"
}
