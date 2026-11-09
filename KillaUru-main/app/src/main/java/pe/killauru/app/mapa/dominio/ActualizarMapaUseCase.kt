package pe.killauru.app.mapa.dominio

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject

/** FR-042, FR-043, FR-044: orquesta la descarga y activación del mapa PMTiles. */
internal class ActualizarMapaUseCase @Inject constructor(
    private val prefs: RepositorioPreferencias,
    private val politica: PoliticaDescarga,
) {
    sealed class Resultado {
        object SinCambios : Resultado()
        object DescargaPendiente : Resultado()
        object Descargando : Resultado()
        object Listo : Resultado()
    }

    suspend fun evaluar(
        versionRemota: String,
        hayWifi: Boolean,
    ): Resultado {
        val versionLocal = prefs.obtenerMapaVersion()
        val usarMoviles = prefs.obtenerUsarDatosMoviles()
        return if (politica.debeDescargar(versionLocal, versionRemota, usarMoviles, hayWifi)) {
            Resultado.DescargaPendiente
        } else {
            Resultado.SinCambios
        }
    }

    suspend fun marcarDescargado(version: String) {
        prefs.guardarMapaVersion(version)
    }
}
