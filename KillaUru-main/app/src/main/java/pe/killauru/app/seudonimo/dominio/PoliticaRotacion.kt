package pe.killauru.app.seudonimo.dominio

import java.time.Duration
import java.time.Instant

/** RN-09 / FR-039: sin ancla confiable no se puede demostrar que venció el plazo. */
internal class PoliticaRotacion(private val vigencia: Duration) {
    fun debeRotar(desde: Instant?, ahora: Instant?): Boolean =
        desde != null && ahora != null && Duration.between(desde, ahora) >= vigencia
}
