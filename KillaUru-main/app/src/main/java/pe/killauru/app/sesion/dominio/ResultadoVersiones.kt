package pe.killauru.app.sesion.dominio

/** FR-002, FR-003: resultado de revisar versiones legales vigentes contra las aceptadas. */
internal sealed class ResultadoVersiones {
    object Vigente : ResultadoVersiones()
    data class PedirAceptacion(
        val terminosVersion: String,
        val politicaVersion: String,
        val terminosTexto: String,
        val politicaTexto: String,
    ) : ResultadoVersiones()
}
