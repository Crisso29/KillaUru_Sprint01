package pe.killauru.app.mapa.dominio

import java.time.Duration
import java.time.Instant

/** Doble local para aislar la sincronización del mapa. */
internal class RelojMapaFijo(private val ahoraMs: Long = 1_000_000L) :
    pe.killauru.app.comun.dominio.Reloj {
    override fun ahoraConfiable(): Instant = Instant.ofEpochMilli(ahoraMs)
    override fun monotonico(): Duration = Duration.ZERO
    override suspend fun anclarGps(utc: Instant, elapsed: Duration) = Unit
    override suspend fun anclarServidor(utc: Instant) = Unit
}
