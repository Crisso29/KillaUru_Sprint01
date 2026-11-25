package pe.killauru.app.deteccion.dominio

/** FR-011, FR-018: descarta ventanas cuyo pico a pico vertical es menor que el umbral (009 §4). */
internal class Prefiltro(private val umbralPicoAPicoMs2: Float) {

    fun pasar(ventana: Ventana): Boolean {
        if (ventana.aV.isEmpty()) return false
        return ventana.aV.max() - ventana.aV.min() >= umbralPicoAPicoMs2
    }
}
