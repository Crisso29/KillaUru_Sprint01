package pe.killauru.app.mapa.datos

import java.io.File
import java.security.MessageDigest
import javax.inject.Inject

/** FR-044: verifica integridad del archivo descargado comparando su SHA-256. */
internal class VerificadorSha256 @Inject constructor() {
    fun verificar(archivo: File, sha256Esperado: String): Boolean {
        val digest = MessageDigest.getInstance("SHA-256")
        archivo.inputStream().use { stream ->
            val buffer = ByteArray(8192)
            var leidos: Int
            while (stream.read(buffer).also { leidos = it } != -1) {
                digest.update(buffer, 0, leidos)
            }
        }
        val calculado = digest.digest().joinToString("") { "%02x".format(it) }
        return calculado.equals(sha256Esperado, ignoreCase = true)
    }
}
