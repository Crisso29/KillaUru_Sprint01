package pe.killauru.app.actualizador.dominio

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject

/** FR-070, FR-071: evalúa y activa un nuevo modelo si la política lo permite. */
internal class ActualizarModeloUseCase @Inject constructor(
    private val prefs: RepositorioPreferencias,
    private val politica: PoliticaActivacionModelo,
) {
    sealed class Resultado {
        object Activado : Resultado()
        object IncompatibleConApp : Resultado()
        object SinCambios : Resultado()
    }

    suspend fun evaluar(manifiesto: ManifiestoModelo): Resultado {
        val versionLocal = prefs.obtenerModeloVersion()
        if (versionLocal == manifiesto.version) return Resultado.SinCambios
        return if (politica.puedeActivar(manifiesto)) {
            prefs.guardarModeloVersion(manifiesto.version)
            Resultado.Activado
        } else {
            Resultado.IncompatibleConApp
        }
    }
}
