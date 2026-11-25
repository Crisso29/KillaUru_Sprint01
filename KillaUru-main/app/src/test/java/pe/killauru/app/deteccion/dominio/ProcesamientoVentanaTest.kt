package pe.killauru.app.deteccion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.soporte.EventosMemoria
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

/**
 * FR-010…FR-013, FR-019: una ventana de sensores pasa por el prefiltro, el modelo y el guardado; y las transiciones de
 * estado de un evento. Una prueba por comportamiento. Patrón AAA.
 */
class ProcesamientoVentanaTest {
    private val ahora = Instant.parse("2026-01-01T00:00:00Z")
    private val fuentes = object : Fuentes {
        override suspend fun seudonimo(horaConfiable: Instant?) = "seudonimo"
        override suspend fun categoria() = CategoriaVehiculo.LIVIANO
        override fun horaConfiable() = ahora
        override fun km(): Double? = null
    }

    // aV con un pico de 5 m/s² → v_pap = 5 > 0.1 → pasa el prefiltro
    private val conMovimiento = Ventana(
        aV = FloatArray(25) { if (it == 12) 5f else 0f },
        aH = FloatArray(25) { 0f },
        w = FloatArray(25) { 0f },
        contextoAv = FloatArray(100) { 0f },
        velocidadKmH = 30f,
        inicioUtc = ahora,
    )

    /** Modelo de prueba: cuenta cuántas veces lo llamaron y responde lo que se le diga. */
    private class ClasificadorDoble(private val respuesta: Clasificacion) : Clasificador {
        var inferencias = 0
        override val version = "1"
        override fun clasificar(ventana: Ventana): Clasificacion = respuesta.also { inferencias++ }
    }

    private fun caso(repo: EventosMemoria, modelo: Clasificador) =
        ProcesarVentanaUseCase(Prefiltro(0.1f), modelo, FusionVentanas(), GuardarEventoUseCase(repo, PoliticaGuardado(0.75), fuentes))

    @Test fun `en reposo el prefiltro no llama al modelo`() = runTest {
        // Arrange — aV = cero → v_pap = 0 < 0.1
        val modelo = ClasificadorDoble(Clasificacion(TipoAnomalia.Bache, 3, 0.9f))
        val enReposo = Ventana(FloatArray(25), FloatArray(25), FloatArray(25), FloatArray(100), 0f, ahora)
        // Act
        caso(EventosMemoria(), modelo).procesar(enReposo, -13.0, -74.0, 5.0, ahora, ahora)
        // Assert — ahorra batería: sin movimiento no hay inferencia
        assertEquals(0, modelo.inferencias)
    }

    @Test fun `una ventana normal pasa por el modelo pero no se guarda`() = runTest {
        // Arrange
        val repo = EventosMemoria()
        val modelo = ClasificadorDoble(Clasificacion(null, 1, 0.99f))
        // Act
        caso(repo, modelo).procesar(conMovimiento, -13.0, -74.0, 5.0, ahora, ahora)
        // Assert
        assertEquals(1, modelo.inferencias)
        assertTrue(repo.eventos.isEmpty())
    }

    @Test fun `un bache detectado se guarda con la version del modelo`() = runTest {
        // Arrange
        val repo = EventosMemoria()
        // Act
        caso(repo, ClasificadorDoble(Clasificacion(TipoAnomalia.Bache, 3, 0.9f))).procesar(conMovimiento, -13.0, -74.0, 5.0, ahora, ahora)
        // Assert
        assertEquals(TipoAnomalia.Bache, repo.eventos.single().tipo)
        assertEquals("1", repo.eventos.single().modeloVersion)
    }

    private val pendiente = Evento("id", ahora, -13.0, -74.0, 5.0, TipoAnomalia.Bache, 3, 0.9, "1", "seudonimo", EstadoLocal.PENDING, ahora)

    @Test fun `una transicion conserva el evento y anota desde cuando`() {
        // Arrange + Act
        val enviando = TransicionEvento.aplicar(pendiente, EstadoLocal.ENVIANDO, ahora.plusSeconds(1))
        // Assert
        assertEquals(pendiente.eventoId, enviando.eventoId)
        assertEquals(ahora.plusSeconds(1), enviando.estadoDesde)
    }

    @Test fun `un evento ya sincronizado no vuelve a pendiente`() {
        // Arrange
        val sincronizado = TransicionEvento.aplicar(
            TransicionEvento.aplicar(pendiente, EstadoLocal.ENVIANDO, ahora), EstadoLocal.SYNCED, ahora.plusSeconds(2))
        // Act + Assert
        assertFailsWith<IllegalArgumentException> { TransicionEvento.aplicar(sincronizado, EstadoLocal.PENDING, ahora.plusSeconds(3)) }
    }

    @Test fun `un pendiente no salta a sincronizado sin enviarse`() {
        // Arrange + Act + Assert — solo un 200 del servidor marca SYNCED, y eso pasa estando en ENVIANDO
        assertFailsWith<IllegalArgumentException> { TransicionEvento.aplicar(pendiente, EstadoLocal.SYNCED, ahora) }
    }
}
