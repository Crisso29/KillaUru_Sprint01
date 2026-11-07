package pe.killauru.app.actualizador.dominio

internal data class ManifiestoModelo(
    val version: String,
    val archivo: String,
    val esquema: Int,
    val etiquetas: List<String>,
    val severidades: Int,
    val versionMinimaApp: String,
)
