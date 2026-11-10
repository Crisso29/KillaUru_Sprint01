package pe.killauru.app.corredor.datos

import android.content.Context
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import pe.killauru.app.corredor.dominio.Corredor
import pe.killauru.app.corredor.dominio.Vertice
import java.io.File
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-055, FR-074: carga la polilínea del corredor PE-28A desde el archivo generado por
 * `generar_corredor.py` (assets o `filesDir` si se descargó una versión nueva).
 *
 * Prioridad: `filesDir/corredor/corredor_pe28a.v*.json` > asset por defecto.
 * Fallo en el archivo → usa el asset del APK como fallback (R-015).
 */
@Singleton
internal class CargadorCorredor @Inject constructor(
    @ApplicationContext private val ctx: Context,
    private val json: Json,
) {
    companion object {
        private const val ASSET_CORREDOR = "corredor/corredor_pe28a.json"
        private const val DIR_CORREDOR = "corredor"
    }

    fun cargar(): Corredor {
        val vertices = leerVertices()
        return Corredor(vertices)
    }

    private fun leerVertices(): List<Vertice> {
        val contenido = leerContenido()
        val dto = json.decodeFromString<CorredorDto>(contenido)
        return dto.vertices.map { Vertice(it.lat, it.lon, it.km) }
    }

    private fun leerContenido(): String {
        // Archivo descargado tiene prioridad sobre el del APK
        val dir = File(ctx.filesDir, DIR_CORREDOR)
        val descargado = dir.listFiles()?.maxByOrNull { it.name }
        if (descargado?.exists() == true) {
            return descargado.readText()
        }
        return ctx.assets.open(ASSET_CORREDOR).bufferedReader().readText()
    }
}

@Serializable
private data class CorredorDto(
    @SerialName("vertices") val vertices: List<VerticeDto>,
)

@Serializable
private data class VerticeDto(
    @SerialName("lat") val lat: Double,
    @SerialName("lon") val lon: Double,
    @SerialName("km") val km: Double,
)
