package pe.killauru.app.seudonimo.dominio

import java.time.Instant

/** FR-039, R-010: obtiene el seudónimo vigente o genera uno nuevo si rotó o no existe. */
internal class ObtenerSeudonimoUseCase(
    private val repositorio: RepositorioSeudonimo,
    private val politica: PoliticaRotacion,
    private val generarUuid: () -> String,
    private val ahoraMs: () -> Long,
) {
    suspend fun obtener(horaConfiable: Instant?): String {
        val actual = repositorio.obtener()
        val desdeMs = repositorio.obtenerDesdeUtcMs()
        return if (actual != null && desdeMs != null &&
            !politica.debeRotar(desdeMs, horaConfiable)
        ) {
            actual
        } else {
            val nuevo = generarUuid()
            repositorio.guardar(nuevo, ahoraMs())
            nuevo
        }
    }
}
