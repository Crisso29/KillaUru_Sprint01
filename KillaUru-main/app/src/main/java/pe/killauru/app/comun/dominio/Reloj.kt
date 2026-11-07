package pe.killauru.app.comun.dominio

import java.time.Duration
import java.time.Instant

/** FR-026: ausencia de hora confiable impide expirar o purgar, pero no enviar. */
internal interface Reloj {
    fun ahoraConfiable(): Instant?
    fun monotonico(): Duration
    suspend fun anclarGps(utc: Instant, elapsed: Duration)
    suspend fun anclarServidor(utc: Instant)
}
