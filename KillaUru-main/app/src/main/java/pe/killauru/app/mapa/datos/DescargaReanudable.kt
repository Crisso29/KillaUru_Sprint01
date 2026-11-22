package pe.killauru.app.mapa.datos

import java.io.File
import kotlinx.coroutines.CancellationException
import okhttp3.ResponseBody
import retrofit2.Response
import java.io.FileOutputStream
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

    /** Cancelar la corrutina se propaga (no es un fallo de descarga); cualquier otro error se devuelve como [Resultado.Fallida]. */
    suspend fun descargar(url: String, nombre: String, sha256: String): Resultado = try {
        intentar(url, nombre, sha256)
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        Resultado.Fallida(e)
    }

    private suspend fun intentar(url: String, nombre: String, sha256: String): Resultado {
        val parcial = almacen.archivoParcial(nombre)
        val inicio = if (parcial.exists()) parcial.length() else 0L
        val resp = api.descargar(url, if (inicio > 0) "bytes=$inicio-" else null)
        // 416: el parcial no vale (ya estaba completo o el servidor cambió el archivo): se borra y se baja entero.
        if (resp.code() == RANGO_INVALIDO) {
            parcial.delete()
            return intentar(url, nombre, sha256)
        }
        check(resp.isSuccessful) { "HTTP ${resp.code()}" }
        escribir(resp, parcial, anexar = resp.code() == PARCIAL && inicio > 0)
        return finalizar(parcial, nombre, sha256)
    }

    /** Con 206 se añade al final del parcial; con 200 se reescribe desde cero. */
    private fun escribir(resp: Response<ResponseBody>, parcial: File, anexar: Boolean) {
        resp.body().use { body ->
            requireNotNull(body).byteStream().use { input ->
                FileOutputStream(parcial, anexar).use { output -> input.copyTo(output) }
            }
        }
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

    private companion object {
        const val PARCIAL = 206
        const val RANGO_INVALIDO = 416
    }
}
