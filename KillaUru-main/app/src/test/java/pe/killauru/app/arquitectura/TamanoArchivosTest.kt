package pe.killauru.app.arquitectura

import java.io.File
import org.junit.Test
import kotlin.test.assertTrue

/** T007 / FR-076: también se ejecuta en JUnit, incluso sin el paso Python de CI. */
class TamanoArchivosTest {
    @Test
    fun `ningun archivo fuente alcanza doscientas lineas`() {
        // Arrange
        val extensiones = setOf("kt", "kts", "py", "sh")
        val excluidos = setOf("build", "schemas", ".gradle", ".kotlin", "__pycache__")
        // Act
        val errores = File(".").walkTopDown().onEnter { it.name !in excluidos }
            .filter { it.isFile && it.extension in extensiones }
            .filter { it.readLines().size >= 200 }.map { it.path }.toList()
        // Assert
        assertTrue(errores.isEmpty(), errores.joinToString())
    }
}
