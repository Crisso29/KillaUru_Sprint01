package pe.killauru.app.deteccion.datos

import android.content.res.AssetManager
import org.tensorflow.lite.Interpreter
import pe.killauru.app.deteccion.dominio.CaracteristicasP1
import pe.killauru.app.deteccion.dominio.Clasificacion
import pe.killauru.app.deteccion.dominio.Clasificador
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.deteccion.dominio.Ventana
import java.nio.ByteBuffer
import java.nio.ByteOrder
import kotlin.math.exp
import kotlin.math.roundToInt

/** FR-011, FR-015: clasifica una ventana con 20 características p1 y escalado de temperatura. */
internal class ClasificadorLiteRt(
    assets: AssetManager,
    private val archivo: String,
    private val clases: List<String>,
    private val temperatura: Float,
    override val version: String,
) : Clasificador {

    private val interpreter: Interpreter by lazy { construirInterpreter(assets) }

    private val inputBuffer: ByteBuffer by lazy {
        ByteBuffer.allocateDirect(20 * Float.SIZE_BYTES).order(ByteOrder.nativeOrder())
    }
    private val tipoBuffer: Array<FloatArray> by lazy { Array(1) { FloatArray(clases.size) } }
    private val severidadBuffer: Array<FloatArray> by lazy { Array(1) { FloatArray(1) } }

    override fun clasificar(ventana: Ventana): Clasificacion {
        val features = CaracteristicasP1.extraer(ventana)
        inputBuffer.clear()
        features.forEach { inputBuffer.putFloat(it) }
        inputBuffer.rewind()
        val (logits, sev) = inferir()
        return decodificar(logits, sev)
    }

    private fun inferir(): Pair<FloatArray, Float?> {
        if (interpreter.outputTensorCount == 1) {
            interpreter.run(inputBuffer, tipoBuffer)
            return tipoBuffer[0] to null
        }
        val out = hashMapOf<Int, Any>(0 to tipoBuffer, 1 to severidadBuffer)
        interpreter.runForMultipleInputsOutputs(arrayOf<Any>(inputBuffer), out)
        return tipoBuffer[0] to severidadBuffer[0][0]
    }

    private fun decodificar(logits: FloatArray, sev: Float?): Clasificacion {
        val escalados = FloatArray(logits.size) { logits[it] / temperatura }
        val probs = softmax(escalados)
        val mejorIdx = probs.indices.maxByOrNull { probs[it] } ?: 0
        val etiqueta = clases.getOrElse(mejorIdx) { "Normal" }
        val tipo = if (etiqueta == "Normal") null
            else runCatching { TipoAnomalia.deTexto(etiqueta) }.getOrNull()
        val severidad = sev?.let { maxOf(1, minOf(5, it.roundToInt())) }
            ?: maxOf(1, minOf(5, (probs[mejorIdx] * 5).toInt() + 1))
        return Clasificacion(tipo, severidad, probs[mejorIdx])
    }

    private fun softmax(x: FloatArray): FloatArray {
        val maxX = x.max()
        val e = FloatArray(x.size) { exp((x[it] - maxX).toDouble()).toFloat() }
        val s = e.sum()
        return FloatArray(x.size) { e[it] / s }
    }

    private fun construirInterpreter(assets: AssetManager): Interpreter {
        val fd = assets.openFd(archivo)
        val buf = fd.createInputStream().channel.map(
            java.nio.channels.FileChannel.MapMode.READ_ONLY, fd.startOffset, fd.declaredLength,
        )
        return Interpreter(buf)
    }
}
