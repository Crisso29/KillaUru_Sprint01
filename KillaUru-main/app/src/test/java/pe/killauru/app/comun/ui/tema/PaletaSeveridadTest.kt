package pe.killauru.app.comun.ui.tema

import org.junit.Test
import kotlin.test.assertEquals

/** Prototipo `.sev`: cada severidad tiene su color (s1…s5); el número va oscuro sobre 1 a 3 y claro sobre 4 y 5. */
class PaletaSeveridadTest {
    private val p = Paleta.clara

    @Test fun `cada severidad usa su propio color, tambien la 1 y la 2`() {
        // Arrange + Act
        val colores = (1..5).map { p.colorSeveridad(it) }
        // Assert
        assertEquals(p.severidades, colores)
    }

    @Test fun `el numero va oscuro sobre 1 a 3 y claro sobre 4 y 5`() {
        // Arrange + Act
        val textos = (1..5).map { p.textoSobreSeveridad(it) }
        // Assert
        assertEquals(List(3) { p.sobreSeveridadBaja } + List(2) { p.sobreSeveridadAlta }, textos)
    }

    /** La severidad viene del servidor (1 a 5); un valor fuera de rango no debe romper la pantalla. */
    @Test fun `una severidad fuera de rango usa el extremo mas cercano`() {
        // Arrange + Act
        val bajo = p.colorSeveridad(0)
        val alto = p.colorSeveridad(9)
        // Assert
        assertEquals(p.severidades.first(), bajo)
        assertEquals(p.severidades.last(), alto)
    }
}
