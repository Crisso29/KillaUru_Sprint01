package pe.killauru.app.sesion.ui

import org.junit.Test
import kotlin.test.assertEquals

/** FR-007 y prototipo «Ajustes»: «Aceptaste la v1.2 el 14 oct 2026», en hora de Lima. Patrón AAA. */
class TextoAceptacionTest {
    @Test fun `muestra la version y la fecha de Lima en que se acepto`() {
        // Arrange — 14 oct 2026 a las 15:00 UTC (10:00 en Lima)
        val estado = EstadoAjustes(terminosVersion = "1.2", terminosAceptadoUtcMs = 1_791_990_000_000L)
        // Act
        val texto = textoAceptacion(estado)
        // Assert
        assertEquals("Aceptaste la v1.2 el 14 oct 2026", texto)
    }

    @Test fun `a las 02 00 UTC en Lima todavia es el dia anterior`() {
        // Arrange — 15 oct 2026 02:00 UTC = 14 oct 21:00 en Lima
        val estado = EstadoAjustes(terminosVersion = "1.2", terminosAceptadoUtcMs = 1_792_029_600_000L)
        // Act + Assert
        assertEquals("Aceptaste la v1.2 el 14 oct 2026", textoAceptacion(estado))
    }

    @Test fun `sin aceptacion lo dice`() {
        // Arrange + Act + Assert
        assertEquals("Aún no aceptados", textoAceptacion(EstadoAjustes()))
    }
}
