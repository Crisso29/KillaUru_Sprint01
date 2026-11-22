package pe.killauru.app.sesion.ui

import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.util.Locale

/** «14 oct 2026» en hora de Lima, para mostrar fechas guardadas (no lee el reloj: recibe el instante). */
internal object FechaCorta {
    private val LIMA = ZoneId.of("America/Lima")
    private val FORMATO = DateTimeFormatter.ofPattern("d MMM yyyy", Locale.forLanguageTag("es-PE"))

    fun deUtcMs(utcMs: Long): String = FORMATO.format(Instant.ofEpochMilli(utcMs).atZone(LIMA)).replace(".", "")
}
