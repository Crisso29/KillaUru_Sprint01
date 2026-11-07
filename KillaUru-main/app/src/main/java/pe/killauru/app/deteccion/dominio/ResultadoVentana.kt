package pe.killauru.app.deteccion.dominio

internal data class ResultadoVentana(
    val inicioMs: Long,
    val clasificacion: Clasificacion,
)
