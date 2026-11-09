package pe.killauru.app.deteccion.dominio

/** FR-011, FR-015: puerto para el clasificador de ventanas de acelerómetro. */
internal interface Clasificador {
    fun clasificar(ventana: Ventana): Clasificacion
    val version: String
}
