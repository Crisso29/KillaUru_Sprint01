package pe.killauru.app.sesion.dominio

/** FR-001, FR-003: validación de doble casilla y control de versiones legales. */
internal data class Consentimiento(
    val terminosVersion: String,
    val politicaVersion: String,
) {
    fun puedeAceptar(terminosMarcado: Boolean, politicaMarcada: Boolean): Boolean =
        terminosMarcado && politicaMarcada

    fun necesitaRenovacion(aceptadas: VersionesAceptadas): Boolean =
        aceptadas.terminosVersion != terminosVersion ||
            aceptadas.politicaVersion != politicaVersion
}

internal data class VersionesAceptadas(
    val terminosVersion: String,
    val politicaVersion: String,
)
