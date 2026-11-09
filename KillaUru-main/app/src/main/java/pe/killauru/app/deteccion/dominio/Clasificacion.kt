package pe.killauru.app.deteccion.dominio

/** FR-002, RF-02: resultado de la inferencia del modelo embebido (ventana de acelerómetro). */
internal data class Clasificacion(
    val tipo: TipoAnomalia?,
    val severidad: Int,
    val confidence: Float,
) {
    val esNormal: Boolean get() = tipo == null
}
