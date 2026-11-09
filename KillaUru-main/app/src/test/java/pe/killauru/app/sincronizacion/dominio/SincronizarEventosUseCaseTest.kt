package pe.killauru.app.sincronizacion.dominio

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flowOf
import kotlinx.coroutines.test.runTest
import pe.killauru.app.comun.dominio.ParametrosApp
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import java.time.Duration
import java.time.Instant
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** T059: FR-022, FR-029, FR-037 — SincronizarEventosUseCase respeta límites. */
class SincronizarEventosUseCaseTest {

    // ---- dobles ----

    private inner class RelojFalso(private val ahora: Instant?) : Reloj {
        override fun ahoraConfiable() = ahora
        override fun monotonico() = Duration.ZERO
        override suspend fun anclarGps(utc: Instant, elapsed: Duration) = Unit
        override suspend fun anclarServidor(utc: Instant) = Unit
    }

    private inner class ApiFalsa : ApiLotes {
        val lotesSeudonimos = mutableListOf<String>()
        override suspend fun enviar(lote: Lote): RespuestaLoteApi {
            lotesSeudonimos.add(lote.seudonimo)
            val dto = RespuestaLoteDto(
                aceptados = lote.eventos.map { it.eventoId },
                rechazados = emptyList(),
                pendientes = emptyList(),
            )
            return RespuestaLoteApi.Exitosa(dto)
        }
    }

    private inner class RepoFalso(eventos: List<Evento> = emptyList()) : RepositorioEventos {
        private val lista = eventos.toMutableList()
        val expirados = mutableListOf<Long>()
        override suspend fun guardar(evento: Evento) = Unit
        override fun pendientes(): Flow<List<Evento>> = flowOf(lista)
        override suspend fun contarPendientes() = lista.size
        override suspend fun contarEnviando() = 0
        override suspend fun tomarLote(seudonimo: String, max: Int) =
            lista.filter { seudonimo.isEmpty() || it.seudonimo == seudonimo }.take(max)
        override suspend fun expirar(anteriorAUtcMs: Long) { expirados.add(anteriorAUtcMs) }
        override suspend fun recuperarEnviando() = Unit
        override suspend fun aplicarEstados(cambios: Map<String, EstadoLocal>) = Unit
    }

    private fun parametros(max: Int = 50) = requireNotNull(
        pe.killauru.app.comun.datos.CargadorParametros.cargar(
            java.io.File("src/main/assets/parametros.json").readText()
        )
    ).copy(maxEventosPorLote = max)


    private fun evento(id: String, seudonimo: String = "seu1") = Evento(
        eventoId = id,
        detectadoUtc = Instant.EPOCH,
        lat = -13.0, lon = -74.0, precisionM = 5.0,
        tipo = TipoAnomalia.Bache, severidad = 2, confidence = 0.9,
        modeloVersion = "1.0", seudonimo = seudonimo,
        estado = EstadoLocal.PENDING, estadoDesde = Instant.EPOCH,
    )

    @Test
    fun `soloVerMapa no envía nada`() = runTest {
        // Arrange
        val repo = RepoFalso(listOf(evento("e1")))
        val api = ApiFalsa()
        val uc = SincronizarEventosUseCase(repo, EnviarLoteUseCase(api, repo), parametros(), RelojFalso(Instant.now()))

        // Act
        uc.ejecutar(soloVerMapa = true, enAhorro = false)

        // Assert
        assertTrue(api.lotesSeudonimos.isEmpty())
    }

    @Test
    fun `ahorro no envía nada`() = runTest {
        // Arrange
        val repo = RepoFalso(listOf(evento("e1")))
        val api = ApiFalsa()
        val uc = SincronizarEventosUseCase(repo, EnviarLoteUseCase(api, repo), parametros(), RelojFalso(Instant.now()))

        // Act
        uc.ejecutar(soloVerMapa = false, enAhorro = true)

        // Assert
        assertTrue(api.lotesSeudonimos.isEmpty())
    }

    @Test
    fun `dos seudónimos producen lotes separados`() = runTest {
        // Arrange
        val eventos = (1..3).map { evento("e$it", "seu1") } + (4..6).map { evento("e$it", "seu2") }
        val repo = RepoFalso(eventos)
        val api = ApiFalsa()
        val uc = SincronizarEventosUseCase(repo, EnviarLoteUseCase(api, repo), parametros(), RelojFalso(Instant.now()))

        // Act
        uc.ejecutar(soloVerMapa = false, enAhorro = false)

        // Assert — se envió un lote por cada seudónimo
        assertTrue("seu1" in api.lotesSeudonimos)
        assertTrue("seu2" in api.lotesSeudonimos)
    }

    @Test
    fun `sin hora confiable no expira`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val api = ApiFalsa()
        val uc = SincronizarEventosUseCase(repo, EnviarLoteUseCase(api, repo), parametros(), RelojFalso(null))

        // Act
        uc.ejecutar(soloVerMapa = false, enAhorro = false)

        // Assert
        assertTrue(repo.expirados.isEmpty())
    }

    @Test
    fun `con hora confiable expira con límite 168 h`() = runTest {
        // Arrange
        val ahora = Instant.parse("2026-11-04T12:00:00Z")
        val esperado = ahora.minusSeconds(168 * 3600).toEpochMilli()
        val repo = RepoFalso()
        val api = ApiFalsa()
        val uc = SincronizarEventosUseCase(repo, EnviarLoteUseCase(api, repo), parametros(), RelojFalso(ahora))

        // Act
        uc.ejecutar(soloVerMapa = false, enAhorro = false)

        // Assert
        assertEquals(1, repo.expirados.size)
        assertEquals(esperado, repo.expirados[0])
    }
}
