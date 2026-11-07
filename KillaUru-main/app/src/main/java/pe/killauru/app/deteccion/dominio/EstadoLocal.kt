package pe.killauru.app.deteccion.dominio

/** data-model §3-4: estados del ciclo de vida de un evento en la app. */
internal enum class EstadoLocal(val codigo: Int) {
    PENDING(0),
    ENVIANDO(1),
    SYNCED(2),
    REJECTED_LOCAL(3),
    EXPIRADO_LOCAL(4);

    companion object {
        fun deCodigo(codigo: Int): EstadoLocal =
            entries.first { it.codigo == codigo }
    }
}
