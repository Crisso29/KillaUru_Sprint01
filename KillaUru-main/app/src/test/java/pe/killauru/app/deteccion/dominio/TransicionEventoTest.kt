package pe.killauru.app.deteccion.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T054 / FR-020, data-model.md §3: máquina de estados de EventoLocal. */
class TransicionEventoTest {

    @Test
    fun `PENDING a ENVIANDO es valida`() {
        // Arrange / Act / Assert
        assertTrue(TransicionEvento.esValida(EstadoLocal.PENDING, EstadoLocal.ENVIANDO))
    }

    @Test
    fun `ENVIANDO a SYNCED es valida`() {
        // Arrange / Act / Assert
        assertTrue(TransicionEvento.esValida(EstadoLocal.ENVIANDO, EstadoLocal.SYNCED))
    }

    @Test
    fun `ENVIANDO a REJECTED_LOCAL es valida`() {
        // Arrange / Act / Assert
        assertTrue(TransicionEvento.esValida(EstadoLocal.ENVIANDO, EstadoLocal.REJECTED_LOCAL))
    }

    @Test
    fun `ENVIANDO a PENDING es valida al cancelar`() {
        // Arrange / Act / Assert
        assertTrue(TransicionEvento.esValida(EstadoLocal.ENVIANDO, EstadoLocal.PENDING))
    }

    @Test
    fun `PENDING a EXPIRADO_LOCAL es valida`() {
        // Arrange / Act / Assert
        assertTrue(TransicionEvento.esValida(EstadoLocal.PENDING, EstadoLocal.EXPIRADO_LOCAL))
    }

    @Test
    fun `SYNCED a PENDING es invalida`() {
        // Arrange / Act / Assert
        assertFalse(TransicionEvento.esValida(EstadoLocal.SYNCED, EstadoLocal.PENDING))
    }

    @Test
    fun `REJECTED_LOCAL a PENDING es invalida`() {
        // Arrange / Act / Assert
        assertFalse(TransicionEvento.esValida(EstadoLocal.REJECTED_LOCAL, EstadoLocal.PENDING))
    }

    @Test
    fun `EXPIRADO_LOCAL a ENVIANDO es invalida`() {
        // Arrange / Act / Assert
        assertFalse(TransicionEvento.esValida(EstadoLocal.EXPIRADO_LOCAL, EstadoLocal.ENVIANDO))
    }

    @Test
    fun `PENDING a SYNCED directo es invalida`() {
        // Arrange / Act / Assert
        assertFalse(TransicionEvento.esValida(EstadoLocal.PENDING, EstadoLocal.SYNCED))
    }
}
