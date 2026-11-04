package pe.killauru.app.arquitectura

import java.io.File
import org.junit.Test
import kotlin.test.assertTrue

/** T007 / FR-026: protege también builds sin análisis de tipos de detekt. */
class RelojProhibidoTest {
    @Test
    fun `solo reloj sistema accede a relojes globales`() {
        // Arrange
        val fuentes = File("src/main/java").walkTopDown()
            .filter { it.extension == "kt" && it.name != "RelojSistema.kt" }
        val llamadas = Regex("(System\\.(currentTimeMillis|nanoTime)|" +
            "(Instant|LocalDateTime|LocalDate|OffsetDateTime|ZonedDateTime)\\.now|" +
            "Clock\\.system\\w*|SystemClock\\.(elapsedRealtime\\w*|uptimeMillis)|Date)\\s*\\(")
        // Act
        val errores = fuentes.filter { llamadas.containsMatchIn(it.readText()) }.toList()
        // Assert
        assertTrue(errores.isEmpty(), errores.joinToString())
    }
}
