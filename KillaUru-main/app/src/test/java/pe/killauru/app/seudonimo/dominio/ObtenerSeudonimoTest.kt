package pe.killauru.app.seudonimo.dominio

import java.time.Duration
import java.time.Instant
import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.seudonimo.datos.RepositorioSeudonimoDataStore
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.*

class ObtenerSeudonimoTest {
    @Test fun `crea conserva y rota solamente con tiempo confiable vencido`() = runTest {
        val prefs = PreferenciasMemoria()
        val inicio = Instant.parse("2026-01-01T00:00:00Z")
        var contador = 0
        val caso = ObtenerSeudonimoUseCase(RepositorioSeudonimoDataStore(prefs),
            PoliticaRotacion(Duration.ofDays(7)), { "id-${++contador}" }, { inicio.toEpochMilli() })
        assertEquals("id-1", caso.obtener(inicio))
        assertEquals(inicio.toEpochMilli(), prefs.desde)
        assertEquals("id-1", caso.obtener(null))
        assertEquals("id-1", caso.obtener(inicio.plusSeconds(6 * 86400)))
        assertEquals("id-2", caso.obtener(inicio.plusSeconds(7 * 86400)))
    }
}
