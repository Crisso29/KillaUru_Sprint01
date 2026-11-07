package pe.killauru.app.comun.datos.red

import kotlinx.serialization.Serializable

/** FR-030: cuerpo de error del servidor; código = «sesion_invalida», «cuenta_desactivada», etc. */
@Serializable
internal data class ProblemaDto(
    val codigo: String? = null,
    val mensaje: String? = null,
)
