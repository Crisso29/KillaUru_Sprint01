package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioPreferencias

/** FR-003: persiste las versiones legales aceptadas por el usuario. */
internal class AceptarVersionesUseCase(
    private val prefs: RepositorioPreferencias,
    private val ahoraUtcMs: () -> Long,
) {
    suspend fun aceptar(terminosVersion: String, politicaVersion: String) {
        val ahora = ahoraUtcMs()
        prefs.guardarTerminosVersion(terminosVersion)
        prefs.guardarTerminosAceptadoUtc(ahora)
        prefs.guardarPoliticaVersion(politicaVersion)
        prefs.guardarPoliticaAceptadoUtc(ahora)
    }
}
