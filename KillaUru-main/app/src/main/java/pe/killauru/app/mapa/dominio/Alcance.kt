package pe.killauru.app.mapa.dominio

/** data-model §4, RN-08: rol del usuario que determina el alcance del mapa. */
internal enum class Alcance(val codigo: Int) {
    Anonimo(0),
    Registrado(1);

    companion object {
        fun deCodigo(codigo: Int): Alcance =
            entries.first { it.codigo == codigo }
    }
}
