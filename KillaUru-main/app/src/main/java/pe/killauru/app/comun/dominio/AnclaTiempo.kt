package pe.killauru.app.comun.dominio

import java.time.Duration
import java.time.Instant

/** R-003: una hora GPS/servidor solo vale dentro del arranque en que se recibió. */
internal data class AnclaTiempo(val utc: Instant, val monotonico: Duration, val arranque: Int) {
    fun hora(actual: Duration, arranqueActual: Int): Instant? =
        if (arranque != arranqueActual || actual < monotonico) null
        else utc.plus(actual.minus(monotonico))
}
