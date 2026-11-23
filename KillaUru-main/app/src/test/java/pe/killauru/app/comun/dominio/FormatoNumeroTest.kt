package pe.killauru.app.comun.dominio

import org.junit.Test
import kotlin.test.assertEquals

/** Números del prototipo: coma decimal sin importar el idioma del celular. Patrón AAA. */
class FormatoNumeroTest {
    @Test fun `coma decimal, punto de miles y sin ceros sobrantes`() {
        // Arrange + Act + Assert
        assertEquals("288,4", FormatoNumero.decimal(288.4, 1))
        assertEquals("1,2", FormatoNumero.decimal(1.2, 1))
        assertEquals("38", FormatoNumero.decimal(38.0, 1))
        assertEquals("2,3", FormatoNumero.decimal(2.25, 1))
        assertEquals("1.250", FormatoNumero.decimal(1250.0, 0))
    }
}
