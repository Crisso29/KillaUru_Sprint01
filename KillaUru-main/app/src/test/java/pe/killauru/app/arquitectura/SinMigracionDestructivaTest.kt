package pe.killauru.app.arquitectura

import java.io.File
import org.junit.Test
import kotlin.test.assertTrue

/** T007 / FR-024: ninguna sobrecarga de migración destructiva está permitida. */
class SinMigracionDestructivaTest {
    @Test
    fun `no existe fallback destructivo en produccion`() {
        // Arrange
        val fuentes = File("src/main/java").walkTopDown().filter { it.extension == "kt" }
        // Act
        val errores = fuentes.filter { "fallbackToDestructiveMigration" in it.readText() }.toList()
        // Assert
        assertTrue(errores.isEmpty(), errores.joinToString())
    }
}
