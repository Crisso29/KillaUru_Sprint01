package pe.killauru.grabador

import java.io.BufferedWriter
import java.io.File
import java.io.FileWriter

/**
 * T018 / FR-003 — escribe un archivo CSV de sensores por bloques con flush automático.
 * El encabezado se escribe en el constructor; flush tras cada bloque (~1 s a 50 Hz).
 */
internal class EscritorCsv(archivo: File, tipo: Tipo) : AutoCloseable {

    enum class Tipo(val encabezado: String) {
        ACELEROMETRO("t_ns,x,y,z"),
        GIROSCOPIO("t_ns,x,y,z"),
        GPS("t_ns,utc_ms,lat,lon,precision_m,velocidad_ms,rumbo"),
        MARCAS("t_ns,origen"),
    }

    private val writer = BufferedWriter(FileWriter(archivo, false))
    private var muestrasEnBloque = 0

    init {
        writer.write(tipo.encabezado)
        writer.newLine()
        writer.flush()
    }

    /** Escribe una fila de sensor (timestamp + valores); flush cada 50 muestras. */
    fun escribir(tNs: Long, valores: FloatArray) {
        val fila = buildString {
            append(tNs)
            valores.forEach { append(','); append(it) }
        }
        writer.write(fila)
        writer.newLine()
        muestrasEnBloque++
        if (muestrasEnBloque >= 50) {
            writer.flush()
            muestrasEnBloque = 0
        }
    }

    /** Escribe una fila de GPS con todos sus campos. */
    fun escribirGps(
        tNs: Long, utcMs: Long, lat: Double, lon: Double,
        precisionM: Float, velocidadMs: Float, rumbo: Float,
    ) {
        writer.write("$tNs,$utcMs,$lat,$lon,$precisionM,$velocidadMs,$rumbo")
        writer.newLine()
        writer.flush()
    }

    /** Escribe una marca del copiloto o sincronización. */
    fun escribirMarca(tNs: Long, origen: String) {
        writer.write("$tNs,$origen")
        writer.newLine()
        writer.flush()
    }

    override fun close() {
        writer.flush()
        writer.close()
    }
}
