package pe.killauru.app.sesion.dominio

/** Una aceptación guardada: qué documento, qué versión y cuándo (ms UTC). */
internal data class Aceptacion(val documento: DocumentoLegal, val version: String, val aceptadoUtcMs: Long)

/**
 * FR-068, HU-24 (contrato 003 `MisDatos`): todo lo que KillaUru guarda de la cuenta. Los viajes y las detecciones no
 * aparecen porque nunca se unen a la cuenta (RNF-10).
 */
internal data class MisDatos(
    val correo: String,
    val rol: Rol,
    val fechaAltaUtcMs: Long,
    val correoVerificado: Boolean,
    val aceptaciones: List<Aceptacion>,
) {
    /** La última aceptación de ese documento, o null si no hay. */
    fun aceptacion(documento: DocumentoLegal): Aceptacion? =
        aceptaciones.filter { it.documento == documento }.maxByOrNull { it.aceptadoUtcMs }
}
