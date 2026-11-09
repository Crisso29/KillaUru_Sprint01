package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.deteccion.dominio.RepositorioEventos
import javax.inject.Inject

/** FR-033: en cada arranque, eventos atascados en ENVIANDO vuelven a PENDING. */
internal class RecuperarEnviandoUseCase @Inject constructor(
    private val repositorio: RepositorioEventos,
) {
    suspend fun ejecutar() { repositorio.recuperarEnviando() }
}
