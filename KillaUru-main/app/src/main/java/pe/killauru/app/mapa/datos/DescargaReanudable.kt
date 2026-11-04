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
        if (resp.isSuccessful) {
            resp.body()!!.byteStream().use { input ->
                parcial.outputStream().use { output -> input.copyTo(output) }
            }
        }
        val destino = almacen.archivoMapa(nombre)
        parcial.renameTo(destino)
        if (verificador.verificar(destino, sha256)) {
            Resultado.Exitosa(destino)
        } else {
            destino.delete()
            Resultado.Fallida(IllegalStateException("SHA-256 no coincide"))
        }
    } catch (e: Exception) {
        Resultado.Fallida(e)
    }
}
