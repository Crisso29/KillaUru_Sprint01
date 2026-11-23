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

    /**
     * Cancelar la corrutina se propaga (no es un fallo de descarga); cualquier otro error se devuelve como
     * [Resultado.Fallida] y el `.parcial` se conserva para reanudar. Con [bytes] (el tamaño del manifiesto) se comprueba
     * el tamaño antes que la huella; [alProgresar] recibe los bytes que ya hay del archivo, incluido lo de antes.
     */
    suspend fun descargar(
        url: String,
        nombre: String,
        sha256: String,
        bytes: Long? = null,
        alProgresar: (Long) -> Unit = {},
    ): Resultado = try {
        intentar(url, nombre, Esperado(sha256, bytes), alProgresar)
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        Resultado.Fallida(e)
    }

    /** Lo que debe medir y cómo debe ser la huella del archivo terminado. */
    private class Esperado(val sha256: String, val bytes: Long?)

    private suspend fun intentar(url: String, nombre: String, esperado: Esperado, alProgresar: (Long) -> Unit): Resultado {
        val parcial = almacen.archivoParcial(nombre)
        val inicio = if (parcial.exists()) parcial.length() else 0L
        val resp = api.descargar(url, if (inicio > 0) "bytes=$inicio-" else null)
        // 416: el parcial no vale (ya estaba completo o el servidor cambió el archivo): se borra y se baja entero.
        if (resp.code() == RANGO_INVALIDO) {
            parcial.delete()
            return intentar(url, nombre, esperado, alProgresar)
        }
        check(resp.isSuccessful) { "HTTP ${resp.code()}" }
        escribir(resp, parcial, if (resp.code() == PARCIAL) inicio else 0L, alProgresar)
        return finalizar(parcial, nombre, esperado)
    }

    /** Con 206 se añade al final del parcial ([desde] > 0); con 200 se reescribe desde cero. Copia por bloques. */
    private fun escribir(resp: Response<ResponseBody>, parcial: File, desde: Long, alProgresar: (Long) -> Unit) {
        var total = desde
        alProgresar(total)
        resp.body().use { body ->
            requireNotNull(body).byteStream().use { entrada ->
                FileOutputStream(parcial, desde > 0).use { salida ->
                    val bloque = ByteArray(BLOQUE_BYTES)
                    var leidos = entrada.read(bloque)
                    while (leidos >= 0) {
                        salida.write(bloque, 0, leidos)
                        total += leidos
                        alProgresar(total)
                        leidos = entrada.read(bloque)
                    }
                }
            }
        }
    }

    private fun finalizar(parcial: File, nombre: String, esperado: Esperado): Resultado {
        if (esperado.bytes != null && parcial.length() != esperado.bytes) {
            parcial.delete() // terminó pero no mide lo que dice el manifiesto: no es el archivo correcto
            return Resultado.Fallida(IllegalStateException("Tamaño ${parcial.length()} ≠ ${esperado.bytes}"))
        }
        val destino = almacen.archivoMapa(nombre)
        check(parcial.renameTo(destino)) { "No se pudo mover la descarga" }
        return if (verificador.verificar(destino, esperado.sha256)) {
            Resultado.Exitosa(destino)
        } else {
            destino.delete()
            Resultado.Fallida(IllegalStateException("SHA-256 no coincide"))
        }
    }

    private companion object {
        const val PARCIAL = 206
        const val RANGO_INVALIDO = 416
        const val BLOQUE_BYTES = 64 * 1024
    }
}
