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
        val respuesta = RespuestaLote(
            aceptados = listOf("id-1", "id-2"),
            duplicados = emptyList(),
            rechazados = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1", "id-2", "id-3"), respuesta)
        // Assert
        assertEquals(AccionEvento.SYNCED, resultado["id-1"])
        assertEquals(AccionEvento.SYNCED, resultado["id-2"])
    }

    /**
     * EQ-02: si se perdió el 200 y la app reenvía, el servidor los devuelve en `duplicados`. Deben quedar SYNCED;
     * si volvieran a PENDING, la app los reenviaría sin fin.
     */
    @Test
    fun `ids en lista duplicados se marcan SYNCED`() {
        // Arrange
        val respuesta = RespuestaLote(
            aceptados = emptyList(),
            duplicados = listOf("id-1"),
            rechazados = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1"), respuesta)
        // Assert
        assertEquals(AccionEvento.SYNCED, resultado["id-1"])
    }

    /**
     * FR-031 y data-model (transiciones): un rechazo con `id` ya tiene respuesta del servidor y no se reenvía,
     * así que es SYNCED. REJECTED_LOCAL queda solo para el rechazo sin `id`, que se ubica por su `indice`.
     */
    @Test
    fun `rechazado con id se marca SYNCED porque el servidor ya respondio`() {
        // Arrange
        val respuesta = RespuestaLote(
            aceptados = emptyList(),
            duplicados = emptyList(),
            rechazados = listOf(Rechazo(id = "id-1", indice = 0, motivo = "FueraDelCorredor")),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1"), respuesta)
        // Assert
        assertEquals(AccionEvento.SYNCED, resultado["id-1"])
    }

    @Test
    fun `id con indice null se marca REJECTED_LOCAL en posicion indicada`() {
        // Arrange
        val ids = listOf("id-0", "id-1", "id-2", "id-3", "id-4", "id-5", "id-6", "id-7")
        val respuesta = RespuestaLote(
            aceptados = emptyList(),
            duplicados = emptyList(),
            rechazados = listOf(Rechazo(id = null, indice = 7, motivo = "EsquemaInvalido")),
        )
        // Act
        val resultado = interprete.interpretar(ids, respuesta)
        // Assert
        assertEquals(AccionEvento.REJECTED_LOCAL, resultado["id-7"])
    }

    @Test
    fun `id no mencionado queda en PENDING`() {
        // Arrange
        val respuesta = RespuestaLote(
            aceptados = listOf("id-1"),
            duplicados = emptyList(),
            rechazados = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1", "id-no-mencionado"), respuesta)
        // Assert
        assertEquals(AccionEvento.PENDING, resultado["id-no-mencionado"])
    }

    @Test
    fun `id ajeno en la respuesta es ignorado`() {
        // Arrange
        val respuesta = RespuestaLote(
            aceptados = listOf("id-ajeno"),
            duplicados = emptyList(),
            rechazados = emptyList(),
        )
        // Act
        val resultado = interprete.interpretar(listOf("id-1"), respuesta)
        // Assert
        assertTrue(!resultado.containsKey("id-ajeno"))
    }
}
