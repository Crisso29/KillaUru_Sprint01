package pe.killauru.app.sincronizacion.dominio

import org.junit.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** T056 / FR-031, D5: interpretación de las tres listas de la respuesta del lote. */
class InterpretarRespuestaLoteTest {
    private val interprete = InterpretarRespuestaLote()

    @Test
    fun `ids en lista aceptados se marcan SYNCED`() {
        // Arrange
        val respuesta = RespuestaLoteDto(
            aceptados = listOf("id-1", "id-2"),
            rechazados = emptyList(),
            pendientes = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1", "id-2", "id-3"), respuesta)
        // Assert
        assertEquals(AccionEvento.SYNCED, resultado["id-1"])
        assertEquals(AccionEvento.SYNCED, resultado["id-2"])
    }

    @Test
    fun `id con indice null se marca REJECTED_LOCAL en posicion indicada`() {
        // Arrange
        val ids = listOf("id-0", "id-1", "id-2", "id-3", "id-4", "id-5", "id-6", "id-7")
        val respuesta = RespuestaLoteDto(
            aceptados = emptyList(),
            rechazados = listOf(RechazoDto(id = null, indice = 7)),
            pendientes = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(ids, respuesta)
        // Assert
        assertEquals(AccionEvento.REJECTED_LOCAL, resultado["id-7"])
    }

    @Test
    fun `id no mencionado queda en PENDING`() {
        // Arrange
        val respuesta = RespuestaLoteDto(
            aceptados = listOf("id-1"),
            rechazados = emptyList(),
            pendientes = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1", "id-no-mencionado"), respuesta)
        // Assert
        assertEquals(AccionEvento.PENDING, resultado["id-no-mencionado"])
    }

    @Test
    fun `id ajeno en la respuesta es ignorado`() {
        // Arrange
        val respuesta = RespuestaLoteDto(
            aceptados = listOf("id-ajeno"),
            rechazados = emptyList(),
            pendientes = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1"), respuesta)
        // Assert
        assertTrue(!resultado.containsKey("id-ajeno"))
    }
}
