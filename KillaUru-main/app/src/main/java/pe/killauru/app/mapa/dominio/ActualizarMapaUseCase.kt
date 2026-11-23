package pe.killauru.app.mapa.dominio

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject

/**
 * FR-041…FR-044, RNF-15: comprueba el manifiesto de la CDN y, si hay versión nueva y la red lo permite, descarga los
 * 4 archivos. La versión nueva solo se activa si llegan **todos** íntegros; si uno falla, sigue el mapa anterior y lo
 * descargado queda como parcial para reanudar.
 */
internal class ActualizarMapaUseCase @Inject constructor(
    private val cdn: CdnMapa,
    private val archivos: ArchivosMapa,
    private val prefs: RepositorioPreferencias,
    private val politica: PoliticaDescarga,
) {
    /** [alProgresar] recibe el porcentaje (0–100) y lo que pesa toda la versión (para «2,4 MB»). */
    suspend fun ejecutar(hayWifi: Boolean, alProgresar: (porcentaje: Int, bytesTotales: Long) -> Unit): ResultadoMapa {
        val instalado = archivos.instalado()
        return when (val r = cdn.manifiesto(archivos.etag().takeIf { instalado != null })) {
            RespuestaManifiesto.SinCambios -> ResultadoMapa.AlDia
            is RespuestaManifiesto.Fallo -> ResultadoMapa.Fallo(r.causa)
            is RespuestaManifiesto.Nuevo -> when {
                r.manifiesto.version == instalado?.version -> ResultadoMapa.AlDia.also { archivos.activar(instalado, r.etag) }
                !politica.debeDescargar(instalado?.version, r.manifiesto.version, prefs.obtenerUsarDatosMoviles(), hayWifi) ->
                    ResultadoMapa.EsperandoWifi(r.manifiesto.bytesTotales)
                else -> descargar(r.manifiesto, r.etag, alProgresar)
            }
        }
    }

    private suspend fun descargar(manifiesto: ManifiestoMapa, etag: String?, alProgresar: (Int, Long) -> Unit): ResultadoMapa {
        var completos = 0L
        val total = manifiesto.bytesTotales
        for (archivo in manifiesto.archivos) {
            val ok = archivos.descargar(archivo) { recibidos -> alProgresar(porcentaje(completos + recibidos, total), total) }
            if (!ok) return ResultadoMapa.Fallo(IllegalStateException("No se pudo descargar ${archivo.nombre}"))
            completos += archivo.bytes
        }
        archivos.activar(manifiesto, etag)
        alProgresar(CIEN, total)
        return ResultadoMapa.Instalado(manifiesto.version)
    }

    private fun porcentaje(parte: Long, total: Long) = if (total <= 0) CIEN else (parte * CIEN / total).toInt().coerceIn(0, CIEN)

    private companion object {
        const val CIEN = 100
    }
}
