package pe.killauru.app.deteccion.dominio

import java.time.Instant
import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.soporte.EventosMemoria
import kotlin.test.*

class ProcesamientoVentanaTest {
    private val ahora = Instant.parse("2026-01-01T00:00:00Z")
    private val fuentes = object : Fuentes {
        override suspend fun seudonimo(horaConfiable: Instant?) = "seudonimo"
        override suspend fun categoria() = CategoriaVehiculo.LIVIANO
        override fun horaConfiable() = ahora
        override fun km(): Double? = null
    }

    @Test fun `prefiltro descarta reposo y solo persiste clasificaciones positivas`() = runTest {
        val repo = EventosMemoria()
        var inferencias = 0
        var resultado = Clasificacion(null, 1, 0.99f)
        val clasificador = object : Clasificador {
            override val version = "1"
            override fun clasificar(ventana: Ventana): Clasificacion { inferencias++; return resultado }
        }
        val guardar = GuardarEventoUseCase(repo, PoliticaGuardado(0.75), fuentes)
        val caso = ProcesarVentanaUseCase(Prefiltro(0.1), clasificador, FusionVentanas(), guardar)
        caso.procesar(Ventana(emptyList(), ahora), -13.0, -74.0, 5.0, ahora, ahora)
        assertEquals(0, inferencias)
        val ventana = Ventana(listOf(floatArrayOf(0f, 0f, 0f), floatArrayOf(5f, 0f, 0f)), ahora)
        caso.procesar(ventana, -13.0, -74.0, 5.0, ahora, ahora)
        assertTrue(repo.eventos.isEmpty())
        resultado = Clasificacion(TipoAnomalia.Bache, 3, 0.9f)
        caso.procesar(ventana, -13.0, -74.0, 5.0, ahora, ahora)
        assertEquals(2, inferencias)
        assertEquals(TipoAnomalia.Bache, repo.eventos.single().tipo)
        assertEquals("1", repo.eventos.single().modeloVersion)
    }

    @Test fun `transiciones preservan datos y rechazan cambios desde estado final`() {
        val evento = Evento("id", ahora, -13.0, -74.0, 5.0, TipoAnomalia.Bache,
            3, 0.9, "1", "seudonimo", EstadoLocal.PENDING, ahora)
        val enviado = TransicionEvento.aplicar(evento, EstadoLocal.ENVIANDO, ahora.plusSeconds(1))
        assertEquals(evento.eventoId, enviado.eventoId)
        assertEquals(ahora.plusSeconds(1), enviado.estadoDesde)
        val finalizado = TransicionEvento.aplicar(enviado, EstadoLocal.SYNCED, ahora.plusSeconds(2))
        assertFailsWith<IllegalArgumentException> {
            TransicionEvento.aplicar(finalizado, EstadoLocal.PENDING, ahora.plusSeconds(3))
        }
        assertFailsWith<IllegalArgumentException> { TransicionEvento.aplicar(evento, EstadoLocal.SYNCED, ahora) }
    }
}
