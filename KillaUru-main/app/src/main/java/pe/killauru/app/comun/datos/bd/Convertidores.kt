package pe.killauru.app.comun.datos.bd


/** data-model §4: códigos enteros estables; prueban CodigosEstablesTest. */
internal object Convertidores {
    // EstadoLocal: PENDING=0, ENVIANDO=1, SYNCED=2, REJECTED_LOCAL=3, EXPIRADO_LOCAL=4
    fun deEstado(v: Int?): Int? = v
    fun aEstado(v: Int?): Int? = v
}
