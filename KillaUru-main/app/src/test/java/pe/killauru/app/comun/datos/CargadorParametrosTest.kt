package pe.killauru.app.comun.datos

import java.io.File
import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertNull

/** T012 / constitución III: nunca arrancar con umbrales escondidos o incompletos. */
class CargadorParametrosTest {
    private val original get() = File("src/main/assets/parametros.json").readText()

    @Test
    fun `lee los parametros versionados del APK`() {
        // Arrange
        val json = original
        // Act
        val parametros = CargadorParametros.cargar(json)
        // Assert
        assertNotNull(parametros)
        assertEquals(0.75, parametros.confidenceMinimo)
        assertEquals(50, parametros.maxEventosPorLote)
    }

    @Test
    fun `rechaza confidence invalida lotes grandes y backoff desordenado`() {
        // Arrange
        val variantes = listOf(original.replace("0.75", "0"), original.replace("0.75", "1.1"),
            original.replace("\"MaxEventosPorLote\": 50", "\"MaxEventosPorLote\": 51"),
            original.replace("30, 120, 600", "120, 30, 600"))
        // Act
        val resultados = variantes.map(CargadorParametros::cargar)
        // Assert
        resultados.forEach { assertNull(it) }
    }

    @Test
    fun `configuracion ausente o ilegible no usa valores por defecto`() {
        // Arrange
        val variantes = listOf("{}", "no es JSON")
        // Act
        val resultados = variantes.map(CargadorParametros::cargar)
        // Assert
        resultados.forEach { assertNull(it) }
    }
}
