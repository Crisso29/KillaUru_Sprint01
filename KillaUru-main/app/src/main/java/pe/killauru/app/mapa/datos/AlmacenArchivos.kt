package pe.killauru.app.mapa.datos

import android.content.Context
import dagger.hilt.android.qualifiers.ApplicationContext
import java.io.File
import javax.inject.Inject

/**
 * FR-044: carpeta privada `filesDir/mapa` con los archivos del mapa (versionados en el nombre), sus `.parcial` y el
 * estado de la versión activa. Ninguna otra app puede leerla.
 */
internal class AlmacenArchivos @Inject constructor(@ApplicationContext private val context: Context) {
    private val dir: File get() = File(context.filesDir, "mapa").apply { mkdirs() }

    fun archivoMapa(nombre: String): File = File(dir, nombre)

    fun archivoParcial(nombre: String): File = File(dir, "$nombre.parcial")

    /** Manifiesto de la versión activa y su ETag (lo escribe `ArchivosMapaLocal`). */
    fun estado(): File = File(dir, ESTADO)

    /** Archivos de datos (sin el estado): los que se borran al activar otra versión. */
    fun listarDatos(): List<File> = dir.listFiles()?.filter { it.isFile && it.name != ESTADO }.orEmpty()

    fun listarMapa(): List<File> = listarDatos().filter { it.extension == "pmtiles" }

    /** Borra todo el mapa (pruebas, y si hiciera falta empezar de cero). */
    fun vaciar() {
        dir.deleteRecursively()
    }

    private companion object {
        const val ESTADO = "instalado.json"
    }
}
