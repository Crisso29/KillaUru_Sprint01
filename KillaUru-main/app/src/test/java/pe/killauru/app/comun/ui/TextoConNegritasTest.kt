package pe.killauru.app.comun.ui

import androidx.compose.ui.text.font.FontWeight
import org.junit.Test
import kotlin.test.assertEquals

/** `**negrita**` de los textos legales. Patrón AAA. */
class TextoConNegritasTest {
    @Test fun `quita las marcas y pone en negrita lo marcado`() {
        // Arrange + Act
        val t = textoConNegritas("KillaUru es una **ayuda**, no una garantía.")
        // Assert
        assertEquals("KillaUru es una ayuda, no una garantía.", t.text)
        val negrita = t.spanStyles.single()
        assertEquals(FontWeight.Bold, negrita.item.fontWeight)
        assertEquals("ayuda", t.text.substring(negrita.start, negrita.end))
    }

    @Test fun `una marca sin cerrar se muestra tal cual`() {
        // Arrange + Act
        val t = textoConNegritas("precio **sin cerrar")
        // Assert
        assertEquals("precio **sin cerrar", t.text)
        assertEquals(0, t.spanStyles.size)
    }
}
