package pe.killauru.app.soporte

import pe.killauru.app.seudonimo.dominio.RepositorioSeudonimo

/** Doble de [RepositorioSeudonimo] con un seudónimo fijo en memoria. */
internal class SeudonimoMemoria(private var actual: String? = null) : RepositorioSeudonimo {
    private var desdeMs: Long? = actual?.let { 0L }

    override suspend fun obtener() = actual
    override suspend fun obtenerDesdeUtcMs() = desdeMs
    override suspend fun guardar(seudonimo: String, desdeUtcMs: Long) {
        actual = seudonimo
        desdeMs = desdeUtcMs
    }
}
