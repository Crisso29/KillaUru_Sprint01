package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import java.time.Duration
import javax.inject.Inject

/** FR-037: expira eventos PENDING/ENVIANDO más viejos que 168 h si hay hora confiable. */
internal class ExpirarPendientesUseCase @Inject constructor(
    private val repositorio: RepositorioEventos,
    private val reloj: Reloj,
) {
    private val ttl = Duration.ofHours(168)

    suspend fun ejecutar() {
        reloj.ahoraConfiable()?.let { ahora ->
            repositorio.expirar(ahora.minus(ttl).toEpochMilli())
        }
    }
}
