package pe.killauru.app.mapa.datos

import android.content.Context
import java.io.File
import javax.inject.Inject

/** FR-044: gestiona el archivo PMTiles en almacenamiento interno de la app. */
internal class AlmacenArchivos @Inject constructor(private val context: Context) {

    private val dir: File get() = File(context.filesDir, "mapa")

    fun archivoMapa(nombre: String): File {
        dir.mkdirs()
        return File(dir, nombre)
    }

    fun archivoParcial(nombre: String): File {
        dir.mkdirs()
        return File(dir, "$nombre.parcial")
    }

    fun listarMapa(): List<File> = dir.listFiles()
        ?.filter { it.extension == "pmtiles" } ?: emptyList()

    fun eliminar(archivo: File) { archivo.delete() }
}
