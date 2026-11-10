package pe.killauru.app.mapa.dominio

import javax.inject.Inject

/**
 * FR-052, RN-21c: elimina las anomalías fuera del tramo demo al cerrar sesión.
 * Se invoca desde [LimpiezaLocalSesion] para que el anónimo no vea datos del registrado.
 */
internal class RecortarAlDemoUseCase @Inject constructor(
    private val repositorio: RepositorioAnomalias,
) {
    suspend fun ejecutar() {
        repositorio.recortarAlDemo()
    }
}
