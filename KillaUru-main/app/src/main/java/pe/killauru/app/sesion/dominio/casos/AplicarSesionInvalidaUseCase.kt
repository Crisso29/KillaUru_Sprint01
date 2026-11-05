package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/** RN-21c, FR-051: cuenta desactivada → borra tokens y recorta al demo. */
internal class AplicarSesionInvalidaUseCase @Inject constructor(
    private val repoSesion: RepositorioSesion,
    private val limpiezaLocal: LimpiezaLocalSesion,
) {
    suspend fun ejecutar() = limpiezaLocal.ejecutar()
}
