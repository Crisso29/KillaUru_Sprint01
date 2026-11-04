package pe.killauru.app.deteccion.dominio

/** data-model §4, RN-01: catálogo de tipos de anomalía. */
internal enum class TipoAnomalia(val codigo: Int, val texto: String) {
    Bache(1, "Bache"),
    Hundimiento(2, "Hundimiento");

    companion object {
        fun deCodigo(codigo: Int): TipoAnomalia =
            entries.first { it.codigo == codigo }

        fun deTexto(texto: String): TipoAnomalia =
            entries.first { it.texto == texto }
    }
}
