package pe.killauru.app.seudonimo.dominio

/** FR-039: puerto para leer y persistir el seudónimo rotativo. */
internal interface RepositorioSeudonimo {
    suspend fun obtener(): String?
    suspend fun obtenerDesdeUtcMs(): Long?
    suspend fun guardar(seudonimo: String, desdeUtcMs: Long)
}
