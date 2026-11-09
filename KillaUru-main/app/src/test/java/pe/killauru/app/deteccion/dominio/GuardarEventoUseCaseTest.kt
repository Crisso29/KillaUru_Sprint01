package pe.killauru.app.deteccion.dominio

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flowOf
import kotlinx.coroutines.runBlocking
import org.junit.Test
import java.time.Instant
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T041 / FR-013, FR-015, R-003, R-018: guardar evento con GPS válido y confianza ok. */
class GuardarEventoUseCaseTest {

    private val guardados = mutableListOf<Evento>()

    private inner class RepoFalso : RepositorioEventos {
        override suspend fun guardar(evento: Evento) { guardados.add(evento) }
        override fun pendientes(): Flow<List<Evento>> = flowOf(emptyList())
        override suspend fun contarPendientes() = 0
        override suspend fun contarEnviando() = 0
        override suspend fun tomarLote(seudonimo: String, max: Int) = emptyList<Evento>()
        override suspend fun expirar(anteriorAUtcMs: Long) {}
        override suspend fun recuperarEnviando() {}
        override suspend fun aplicarEstados(cambios: Map<String, EstadoLocal>) {}
    }

    private val t0 = Instant.parse("2026-01-01T12:00:00Z")

    private inner class FuentesFalsas(private val hora: Instant? = t0) : Fuentes {
        override suspend fun seudonimo(horaConfiable: Instant?) = "uuid-seudonimo"
        override suspend fun categoria() = CategoriaVehiculo.LIVIANO
        override fun horaConfiable() = hora
        override fun km() = 100.0
    }

    @Test
    fun `guarda evento con confianza ok y GPS fresco`() = runBlocking {
        // Arrange
        val uc = GuardarEventoUseCase(RepoFalso(), PoliticaGuardado(0.75), FuentesFalsas())
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.80f)
        // Act
        val guardado = uc.guardar(c, -13.1, -74.2, 5.0, t0, "0.1.0", t0)
        // Assert
        assertTrue(guardado)
        assertEquals(1, guardados.size)
        assertEquals(EstadoLocal.PENDING, guardados[0].estado)
    }

    @Test
    fun `fix de 2001 ms no guarda`() = runBlocking {
        // Arrange
        val uc = GuardarEventoUseCase(RepoFalso(), PoliticaGuardado(0.75), FuentesFalsas())
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.80f)
        val fixViejo = t0.minusMillis(2001)
        // Act
        val guardado = uc.guardar(c, -13.1, -74.2, 5.0, fixViejo, "0.1.0", t0)
        // Assert
        assertFalse(guardado)
        assertTrue(guardados.isEmpty())
    }

    @Test
    fun `sin hora confiable no guarda`() = runBlocking {
        // Arrange
        val uc = GuardarEventoUseCase(RepoFalso(), PoliticaGuardado(0.75), FuentesFalsas(hora = null))
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.80f)
        // Act
        val guardado = uc.guardar(c, -13.1, -74.2, 5.0, t0, "0.1.0", t0)
        // Assert
        assertFalse(guardado)
    }

    @Test
    fun `seudonimo vigente se usa al guardar`() = runBlocking {
        // Arrange
        val uc = GuardarEventoUseCase(RepoFalso(), PoliticaGuardado(0.75), FuentesFalsas())
        val c = Clasificacion(TipoAnomalia.Bache, 3, 0.80f)
        // Act
        uc.guardar(c, -13.1, -74.2, 5.0, t0, "0.1.0", t0)
        // Assert
        assertEquals("uuid-seudonimo", guardados[0].seudonimo)
    }
}
