package pe.killauru.app.actualizador.dominio

/** Campos del manifiesto del modelo que necesita el Actualizador y la detección (009 schema §calibracion §prefiltro). */
internal data class ManifiestoModelo(
    val version: String,
    val archivo: String,
    val clases: List<String>,
    val versionMinimaApp: String,
    val temperatura: Float,
    val umbralPicoAPicoMs2: Float,
)
