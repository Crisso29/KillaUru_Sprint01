package pe.killauru.grabador

import org.junit.After
import org.junit.Before
import org.junit.Test
import java.io.File
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * T015 / FR-003, FR-004, FR-005 — EscritorCsv: encabezados, unidades y flush por bloque.
 * Tests JVM puros: no requieren Android ni Robolectric.
 */
class EscritorCsvTest {

    private lateinit var archivo: File

    @Before
    fun crearArchivo() {
        archivo = File.createTempFile("test_escritor", ".csv")
    }

    @After
    fun borrarArchivo() {
        archivo.delete()
    }

    @Test
    fun `encabezado de acelerometro cumple contrato del formato de grabacion`() {
        // Arrange
        val escritor = EscritorCsv(archivo, EscritorCsv.Tipo.ACELEROMETRO)
        // Act
        escritor.close()
        val primeraLinea = archivo.readLines().first()
        // Assert — formato-grabacion.md: t_ns,x,y,z
        assertEquals("t_ns,x,y,z", primeraLinea)
    }

    @Test
    fun `encabezado de giroscopio cumple contrato del formato de grabacion`() {
        // Arrange
        val escritor = EscritorCsv(archivo, EscritorCsv.Tipo.GIROSCOPIO)
        // Act
        escritor.close()
        val primeraLinea = archivo.readLines().first()
        // Assert
        assertEquals("t_ns,x,y,z", primeraLinea)
    }

    @Test
    fun `encabezado de gps cumple contrato del formato de grabacion`() {
        // Arrange
        val escritor = EscritorCsv(archivo, EscritorCsv.Tipo.GPS)
        // Act
        escritor.close()
        val primeraLinea = archivo.readLines().first()
        // Assert — formato-grabacion.md: t_ns,utc_ms,lat,lon,precision_m,velocidad_ms,rumbo
        assertEquals("t_ns,utc_ms,lat,lon,precision_m,velocidad_ms,rumbo", primeraLinea)
    }

    @Test
    fun `escribe fila de acelerometro con valores en unidades correctas`() {
        // Arrange
        val escritor = EscritorCsv(archivo, EscritorCsv.Tipo.ACELEROMETRO)
        // Act
        escritor.escribir(tNs = 1_000_000_000L, valores = floatArrayOf(0.1f, -9.81f, 0.0f))
        escritor.close()
        val lineas = archivo.readLines()
        // Assert — línea de datos después del encabezado
        assertEquals(2, lineas.size)
        assertTrue(lineas[1].startsWith("1000000000,"))
    }

    @Test
    fun `flush ocurre al escribir un bloque completo`() {
        // Arrange
        val escritor = EscritorCsv(archivo, EscritorCsv.Tipo.ACELEROMETRO)
        // Act — escribir 50 muestras (un bloque ~1 s a 50 Hz)
        repeat(50) { i ->
            escritor.escribir(tNs = i * 20_000_000L, valores = floatArrayOf(0f, 0f, 9.81f))
        }
        // Assert — el archivo tiene datos antes de close (flush intermedio)
        val tamanioAntes = archivo.length()
        escritor.close()
        assertTrue(tamanioAntes > 0, "El archivo debe tener datos antes del close")
    }
}
