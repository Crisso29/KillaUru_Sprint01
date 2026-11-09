package pe.killauru.app.historial.dominio

import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue

/** T115 / FR-069: acumula recorridos y los purga a 6 meses + 1 día. */
class AcumuladorRecorridoTest {
    private val acumulador = AcumuladorRecorrido()

    @Test
    fun `viaje registrado con 7 eventos acumula correctamente`() {
        // Arrange
        val viaje = ViajeRecorrido(
            id = "viaje-1", inicioMs = 0L, finMs = 3600_000L,
            kmDesde = 268.0, kmHasta = 335.073, eventos = 7,
        )
        // Act
        acumulador.registrar(viaje, registrado = true)
        // Assert
        val recorridos = acumulador.todos()
        assertEquals(1, recorridos.size)
        assertEquals(7, recorridos[0].eventos)
    }

    @Test
    fun `anonimo no guarda recorrido`() {
        // Arrange
        val viaje = ViajeRecorrido(
            id = "viaje-anon", inicioMs = 0L, finMs = 3600_000L,
            kmDesde = 268.0, kmHasta = 335.073, eventos = 3,
        )
        // Act
        acumulador.registrar(viaje, registrado = false)
        // Assert
        assertTrue(acumulador.todos().isEmpty())
    }

    @Test
    fun `ida y vuelta suma kmRecorridos`() {
        // Arrange
        acumulador.registrar(ViajeRecorrido("v1", 0L, 1000L, 268.0, 335.073, 2), registrado = true)
        acumulador.registrar(ViajeRecorrido("v2", 2000L, 3000L, 335.073, 268.0, 1), registrado = true)
        // Act
        val km = acumulador.todos().sumOf { it.kmRecorridos }
        // Assert
        assertEquals(2 * (335.073 - 268.0), km, 0.001)
    }

    @Test
    fun `recorrido de mas de 6 meses mas 1 dia se purga`() {
        // Arrange
        val sesMesesMasUnDiaMs = (6 * 30 + 1).toLong() * 24 * 60 * 60 * 1000L
        val vieje = ViajeRecorrido("viejo", 0L, 1000L, 268.0, 335.073, 1)
        acumulador.registrar(vieje, registrado = true)
        // Act
        acumulador.purgar(ahoraMs = sesMesesMasUnDiaMs + 2000L)
        // Assert
        assertTrue(acumulador.todos().isEmpty())
    }
}
