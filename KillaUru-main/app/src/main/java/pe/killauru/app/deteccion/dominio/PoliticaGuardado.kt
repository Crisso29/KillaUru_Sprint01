package pe.killauru.app.deteccion.dominio

/** RN-04, FR-012: confidence mínima 0.75 y tipo no nulo para guardar el evento. */
internal class PoliticaGuardado(private val confidenceMinimo: Double) {
    fun debeGuardar(c: Clasificacion): Boolean =
        c.tipo != null && c.confidence >= confidenceMinimo
}
