package pe.killauru.app.sesion.dominio

import org.junit.Test
import kotlin.test.assertEquals

/** T105 / FR-060 / D13: límites del contrato de identidad, sin llamadas HTTP. */
class ValidadorContrasenaTest {
    @Test
    fun `acepta de diez a ciento veintiocho caracteres`() {
        // Arrange
        val validador = ValidadorContrasena(10, 128)
        // Act
        val resultados = listOf(9, 10, 128, 129).map { validador.validar("x".repeat(it)) }
        // Assert
        assertEquals(listOf(false, true, true, false), resultados)
    }
}
