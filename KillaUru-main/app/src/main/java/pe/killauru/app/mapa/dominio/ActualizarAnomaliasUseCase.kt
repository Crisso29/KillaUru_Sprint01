package pe.killauru.app.mapa.dominio

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import javax.inject.Inject

/** FR-040, FR-049: actualiza anomalías y recorta al demo si corresponde. */
internal class ActualizarAnomaliasUseCase @Inject constructor(
    private val repositorio: RepositorioAnomalias,
) {
    suspend fun actualizar(altas: List<AnomaliaLocal>, bajas: List<String>) {
        repositorio.actualizarLote(altas, bajas)
    }

    suspend fun recortarAlDemo() {
        repositorio.recortarAlDemo()
    }
}
