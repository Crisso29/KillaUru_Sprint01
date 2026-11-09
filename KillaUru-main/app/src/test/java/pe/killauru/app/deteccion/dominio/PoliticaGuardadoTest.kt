package pe.killauru.app.deteccion.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T039 / FR-012, RN-04: confidence >= 0.75 y tipo no nulo para guardar. */
class PoliticaGuardadoTest {
    private val politica = PoliticaGuardado(confidenceMinimo = 0.75)

    @Test
    fun `confidence 0_74 no debe guardarse`() {
        // Arrange
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.74f)
        // Act / Assert
        assertFalse(politica.debeGuardar(c))
    }

    @Test
    fun `confidence 0_75 exacto debe guardarse`() {
        // Arrange
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.75f)
        // Act / Assert
        assertTrue(politica.debeGuardar(c))
    }

    @Test
    fun `confidence 0_76 debe guardarse`() {
        // Arrange
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.76f)
        // Act / Assert
        assertTrue(politica.debeGuardar(c))
    }

    @Test
    fun `clasificacion Normal con 0_99 no debe guardarse`() {
        // Arrange
        val c = Clasificacion(tipo = null, severidad = 1, confidence = 0.99f)
        // Act / Assert
        assertFalse(politica.debeGuardar(c))
    }
}
