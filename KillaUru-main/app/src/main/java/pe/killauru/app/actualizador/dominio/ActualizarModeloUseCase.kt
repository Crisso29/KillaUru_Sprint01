package pe.killauru.app.actualizador.dominio

import javax.inject.Inject

/** FR-070, FR-071: descarga y activa un nuevo modelo si la política lo permite. */
internal class ActualizarModeloUseCase @Inject constructor(
    private val politica: PoliticaActivacionModelo,
) {
    suspend fun ejecutar(remoto: ManifiestoModelo, local: ManifiestoModelo?, enViaje: Boolean): Boolean {
        if (!politica.puedeActivar(remoto)) return false
        if (local != null && remoto.version == local.version) return false
        if (enViaje) return false
        return true
    }
}
