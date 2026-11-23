package pe.killauru.app.seudonimo.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.seudonimo.datos.RepositorioSeudonimoDataStore
import pe.killauru.app.soporte.PreferenciasMemoria
import java.time.Duration
import java.time.Instant
import kotlin.test.assertEquals

/** D2, RNF-10: el seudónimo se crea una vez y solo rota a los 7 días medidos con hora confiable. Patrón AAA. */
class ObtenerSeudonimoTest {
    private val inicio = Instant.parse("2026-01-01T00:00:00Z")
    private val prefs = PreferenciasMemoria()
    private var contador = 0
    private val caso = ObtenerSeudonimoUseCase(RepositorioSeudonimoDataStore(prefs), PoliticaRotacion(Duration.ofDays(7)),
        { "id-${++contador}" }, { inicio.toEpochMilli() })

    @Test fun `la primera vez crea el seudonimo y guarda desde cuando`() = runTest {
        // Arrange + Act
        val seudonimo = caso.obtener(inicio)
        // Assert
        assertEquals("id-1", seudonimo)
        assertEquals(inicio.toEpochMilli(), prefs.desde)
    }

    @Test fun `sin hora confiable conserva el que hay`() = runTest {
        // Arrange
        caso.obtener(inicio)
        // Act
        val seudonimo = caso.obtener(null)
        // Assert — no se rota con un reloj en el que no se confía
        assertEquals("id-1", seudonimo)
    }

    @Test fun `antes de 7 dias lo conserva y a los 7 lo rota`() = runTest {
        // Arrange
        caso.obtener(inicio)
        // Act
        val alSexto = caso.obtener(inicio.plusSeconds(6 * 86_400L))
        val alSeptimo = caso.obtener(inicio.plusSeconds(7 * 86_400L))
        // Assert
        assertEquals("id-1", alSexto)
        assertEquals("id-2", alSeptimo)
    }
}
