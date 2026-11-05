package pe.killauru.app.sincronizacion.dominio

import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** T057 / FR-032, FR-033, R-026: lógica de partición y manejo de errores 400/413. */
class PlanParticionTest {
    private val plan = PlanParticion(maxPorLote = 50)

    @Test
    fun `400 items en lotes de 50 produce 8 lotes de 50`() {
        // Arrange
        val ids = (1..400).map { "id-$it" }
        // Act
        val lotes = plan.partir(ids)
        // Assert
        assertEquals(8, lotes.size)
        assertTrue(lotes.all { it.size == 50 })
    }

    @Test
    fun `51 items da 2 lotes`() {
        // Arrange
        val ids = (1..51).map { "id-$it" }
        // Act
        val lotes = plan.partir(ids)
        // Assert
        assertEquals(2, lotes.size)
        assertEquals(50, lotes[0].size)
        assertEquals(1, lotes[1].size)
    }

    @Test
    fun `al partir culpable 400 lote de 25 en 2 de 12 y 13`() {
        // Arrange
        val ids = (1..25).map { "id-$it" }
        // Act
        val sublotes = plan.partirCulpable(ids)
        // Assert
        assertEquals(2, sublotes.size)
    }

    @Test
    fun `lote culpable de 1 elemento con error 400 se marca REJECTED_LOCAL`() {
        // Arrange
        val ids = listOf("id-culpable")
        // Act
        val sublotes = plan.partirCulpable(ids)
        // Assert
        assertTrue(sublotes.isEmpty(), "Un lote de 1 no se puede subdividir mas")
    }
}
