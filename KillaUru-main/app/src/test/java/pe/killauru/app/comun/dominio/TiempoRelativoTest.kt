package pe.killauru.app.comun.dominio

import java.time.Instant
import org.junit.Test
import kotlin.test.assertEquals

/** T011 / FR-047: español y desfase UTC−5, independientes de la zona del teléfono. */
class TiempoRelativoTest {
    @Test
    fun `minutos horas ayer y fecha usan la hora peruana`() {
        // Arrange
        val ahora = Instant.parse("2026-10-20T18:00:00Z")
        val casos = mapOf(ahora.minusSeconds(59) to "hace un momento",
            ahora.minusSeconds(60) to "hace 1 min", ahora.minusSeconds(3540) to "hace 59 min",
            ahora.minusSeconds(7200) to "hace 2 horas",
            Instant.parse("2026-10-19T20:30:00Z") to "ayer a las 15:30",
            Instant.parse("2026-10-18T14:05:00Z") to "el 18 oct a las 09:05")
        // Act
        val resultados = casos.keys.map { TiempoRelativo.formatear(it, ahora) }
        // Assert
        assertEquals(casos.values.toList(), resultados)
    }
}
