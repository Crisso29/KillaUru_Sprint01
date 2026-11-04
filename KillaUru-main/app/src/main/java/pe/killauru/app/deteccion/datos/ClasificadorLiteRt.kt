package pe.killauru.app.deteccion.datos

import android.content.res.AssetManager
import org.tensorflow.lite.Interpreter
import pe.killauru.app.deteccion.dominio.Clasificacion
import pe.killauru.app.deteccion.dominio.Clasificador
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.deteccion.dominio.Ventana
import java.nio.ByteBuffer
import java.nio.ByteOrder

/** FR-011, FR-015: clasifica una ventana de acelerómetro con el modelo LiteRT embebido. */
internal class ClasificadorLiteRt(
    assets: AssetManager,
    private val archivo: String,
    private val etiquetas: List<String>,
    private val nSeveridades: Int,
    private val normalizador: NormalizadorEntrada,
    override val version: String,
) : Clasificador {

    private val interpreter: Interpreter by lazy {
        val fd = assets.openFd(archivo)
        val buffer = fd.createInputStream().channel.map(
            java.nio.channels.FileChannel.MapMode.READ_ONLY,
            fd.startOffset, fd.declaredLength
        )
        Interpreter(buffer)
    }

    private val outputBuffer: Array<FloatArray> by lazy {
        Array(1) { FloatArray(etiquetas.size * nSeveridades) }
    }

    override fun clasificar(ventana: Ventana): Clasificacion {
        val entrada = normalizador.normalizar(ventana.muestras)
        val inputBuffer = ByteBuffer.allocateDirect(entrada.size * 4)
            .order(ByteOrder.nativeOrder())
        entrada.forEach { inputBuffer.putFloat(it) }
        inputBuffer.rewind()
        interpreter.run(inputBuffer, outputBuffer)
        val scores = outputBuffer[0]
        val mejorIdx = scores.indices.maxByOrNull { scores[it] } ?: 0
        val tipoIdx = mejorIdx / nSeveridades
        val severidad = (mejorIdx % nSeveridades) + 1
        val etiqueta = etiquetas.getOrElse(tipoIdx) { "Normal" }
        val tipo = if (etiqueta == "Normal") null else
            runCatching { TipoAnomalia.deTexto(etiqueta) }.getOrNull()
        return Clasificacion(tipo, severidad, scores[mejorIdx])
    }
}
