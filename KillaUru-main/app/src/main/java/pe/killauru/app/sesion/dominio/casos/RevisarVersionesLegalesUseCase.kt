package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.ResultadoVersiones
import pe.killauru.app.sesion.dominio.VersionesAceptadas
import pe.killauru.app.sesion.dominio.Consentimiento

/** FR-002, FR-003, R-024: sin red usa versión del APK como versión vigente. */
internal class RevisarVersionesLegalesUseCase(
    private val prefs: RepositorioPreferencias,
    private val terminosApk: String,
    private val politicaApk: String,
    private val terminosTexto: String = "",
    private val politicaTexto: String = "",
) {
    suspend fun revisar(): ResultadoVersiones {
        val tAceptado = prefs.obtenerTerminosVersion()
        val pAceptada = prefs.obtenerPoliticaVersion()
        val consentimiento = Consentimiento(terminosApk, politicaApk)
        val aceptadas = VersionesAceptadas(
            terminosVersion = tAceptado ?: "",
            politicaVersion = pAceptada ?: "",
        )
        return if (!consentimiento.necesitaRenovacion(aceptadas)) {
            ResultadoVersiones.Vigente
        } else {
            ResultadoVersiones.PedirAceptacion(
                terminosVersion = terminosApk,
                politicaVersion = politicaApk,
                terminosTexto = terminosTexto,
                politicaTexto = politicaTexto,
            )
        }
    }
}
