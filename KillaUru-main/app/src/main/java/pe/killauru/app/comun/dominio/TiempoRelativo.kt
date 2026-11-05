package pe.killauru.app.comun.dominio

import java.time.Duration
import java.time.Instant
import java.time.ZoneOffset
import java.time.format.DateTimeFormatter
import java.util.Locale

/** R-031 / RNF-14: presentación en español, con día local UTC−5 fijo (RN-12). */
internal object TiempoRelativo {
    private val zona = ZoneOffset.of("-05:00")
    private val hora = DateTimeFormatter.ofPattern("HH:mm", Locale.forLanguageTag("es-PE"))
    private val fecha = DateTimeFormatter.ofPattern("d MMM", Locale.forLanguageTag("es-PE"))

    fun formatear(instante: Instant, ahora: Instant): String {
        val diferencia = Duration.between(instante, ahora)
        if (diferencia.seconds < 60) return "hace un momento"
        if (diferencia.toMinutes() < 60) return "hace ${diferencia.toMinutes()} min"
        return formatearFecha(instante, ahora, diferencia.toHours())
    }

    private fun formatearFecha(instante: Instant, ahora: Instant, horas: Long): String {
        val local = instante.atOffset(zona)
        val hoy = ahora.atOffset(zona).toLocalDate()
        return when (local.toLocalDate()) {
            hoy -> "hace $horas ${if (horas == 1L) "hora" else "horas"}"
            hoy.minusDays(1) -> "ayer a las ${hora.format(local)}"
            else -> "el ${fecha.format(local)} a las ${hora.format(local)}"
        }
    }
}
