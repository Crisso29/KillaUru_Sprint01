package pe.killauru.app.mapa.datos

import java.io.File
import javax.inject.Inject

/** FR-044: descarga un archivo con soporte de reanudación por HTTP Range. */
internal class DescargaReanudable @Inject constructor(
    private val api: CdnApi,
    private val almacen: AlmacenArchivos,
    private val verificador: VerificadorSha256,
) {
    sealed class Resultado {
        data class Exitosa(val archivo: File) : Resultado()
        data class Fallida(val causa: Throwable) : Resultado()
    }

    suspend fun descargar(url: String, nombre: String, sha256: String): Resultado = try {
        val parcial = almacen.archivoParcial(nombre)
        val inicio = if (parcial.exists()) parcial.length() else 0L
        val rango = if (inicio > 0) "bytes=$inicio-" else null
        val resp = api.descargar(url, rango)

        // 416: el parcial es inválido (ya fue completo o el servidor reinició la secuencia).
        // Borramos y hacemos una descarga completa desde cero.
        if (resp.code() == 416) {
            parcial.delete()
            return descargar(url, nombre, sha256)
        }

        check(resp.isSuccessful) { "HTTP ${resp.code()}" }
        resp.body().use { body ->
            requireNotNull(body).byteStream().use { input ->
                java.io.FileOutputStream(parcial, resp.code() == 206 && inicio > 0).use { output ->
                    input.copyTo(output)
                }
            }
        }
        finalizar(parcial, nombre, sha256)
    } catch (e: Exception) {
        Resultado.Fallida(e)
    }
    private fun finalizar(parcial: File, nombre: String, sha256: String): Resultado {
        val destino = almacen.archivoMapa(nombre)
        check(parcial.renameTo(destino)) { "No se pudo mover la descarga" }
        return if (verificador.verificar(destino, sha256)) {
            Resultado.Exitosa(destino)
        } else {
            destino.delete()
            Resultado.Fallida(IllegalStateException("SHA-256 no coincide"))
        }
    }
}
