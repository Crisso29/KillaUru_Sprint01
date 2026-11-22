package pe.killauru.app.sincronizacion.dominio

import kotlinx.coroutines.test.runTest
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.deteccion.dominio.RepositorioEventos
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import java.time.Instant
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flowOf

/** T058: FR-034, R-008 — EnviarLoteUseCase maneja 401/403 correctamente. */
class EnviarLoteUseCaseTest {

    private val e1 = evento("e1")
    private val e2 = evento("e2")

    // ---- dobles ----

    private inner class ApiFalsa(
        private val respuesta: RespuestaLoteApi,
    ) : ApiLotes {
        var lotesEnviados = 0
        override suspend fun enviar(lote: Lote): RespuestaLoteApi {
            lotesEnviados++
            return respuesta
        }
    }

    private inner class RepoFalso : RepositorioEventos {
        val estadosAplicados = mutableMapOf<String, EstadoLocal>()
        override suspend fun guardar(evento: Evento) = Unit
        override fun pendientes(): Flow<List<Evento>> = flowOf(emptyList())
        override suspend fun contarPendientes() = 0
        override suspend fun contarEnviando() = 0
        override suspend fun tomarLote(seudonimo: String, max: Int) = emptyList<Evento>()
        override suspend fun expirar(anteriorAUtcMs: Long) = Unit
        override suspend fun recuperarEnviando() = Unit
        override suspend fun aplicarEstados(cambios: Map<String, EstadoLocal>) {
            estadosAplicados.putAll(cambios)
        }
    }

    /**
     * FR-031: cada rechazo del 200 se suma a su motivo en `contador_local`, tenga `id` o no.
     * El estado del evento lo decide [InterpretarRespuestaLote]; aquí solo se cuenta.
     */
    @Test
    fun `200 suma cada rechazo a su motivo con id o sin id`() = runTest {
        // Arrange
        val contador = ContadorRechazosEnMemoria()
        val respuesta = RespuestaLote(
            aceptados = emptyList(),
            duplicados = emptyList(),
            rechazados = listOf(Rechazo("e1", 0, "FueraDelCorredor"), Rechazo(null, 1, "EsquemaInvalido")),
        )
        val uc = EnviarLoteUseCase(ApiFalsa(RespuestaLoteApi.Exitosa(respuesta)), RepoFalso(), contador)

        // Act
        uc.ejecutar("seu1", listOf("e1", "e2"), listOf(e1, e2))

        // Assert
        assertEquals(listOf("FueraDelCorredor", "EsquemaInvalido"), contador.motivos)
    }

    @Test
    fun `401 pone todos en PENDING sin REJECTED_LOCAL`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val api = ApiFalsa(RespuestaLoteApi.Error401("token_invalido"))
        val uc = EnviarLoteUseCase(api, repo, ContadorRechazosEnMemoria())

        // Act
        uc.ejecutar("seu1", listOf("e1", "e2"), listOf(e1, e2))

        // Assert
        assertEquals(EstadoLocal.PENDING, repo.estadosAplicados["e1"])
        assertEquals(EstadoLocal.PENDING, repo.estadosAplicados["e2"])
    }

    @Test
    fun `403 pone todos en PENDING sin REJECTED_LOCAL`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val api = ApiFalsa(RespuestaLoteApi.Error403("sesion_invalida"))
        val uc = EnviarLoteUseCase(api, repo, ContadorRechazosEnMemoria())

        // Act
        uc.ejecutar("seu1", listOf("e1", "e2"), listOf(e1, e2))

        // Assert
        assertEquals(EstadoLocal.PENDING, repo.estadosAplicados["e1"])
        assertEquals(EstadoLocal.PENDING, repo.estadosAplicados["e2"])
    }

    @Test
    fun `200 exitosa aplica SYNCED a los ids confirmados`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val dto = RespuestaLote(
            aceptados = listOf("e1", "e2"),
            duplicados = emptyList(),
            rechazados = emptyList(),
        )
        val api = ApiFalsa(RespuestaLoteApi.Exitosa(dto))
        val uc = EnviarLoteUseCase(api, repo, ContadorRechazosEnMemoria())

        // Act
        uc.ejecutar("seu1", listOf("e1", "e2"), listOf(e1, e2))

        // Assert
        assertEquals(EstadoLocal.SYNCED, repo.estadosAplicados["e1"])
        assertEquals(EstadoLocal.SYNCED, repo.estadosAplicados["e2"])
    }

    @Test
    fun `error de red pone todos en PENDING`() = runTest {
        // Arrange
        val repo = RepoFalso()
        val api = ApiFalsa(RespuestaLoteApi.ErrorRed(RuntimeException("sin red")))
        val uc = EnviarLoteUseCase(api, repo, ContadorRechazosEnMemoria())

        // Act
        uc.ejecutar("seu1", listOf("e1"), listOf(e1))

        // Assert
        assertEquals(EstadoLocal.PENDING, repo.estadosAplicados["e1"])
    }

    private fun evento(id: String) = Evento(
        eventoId = id,
        detectadoUtc = Instant.EPOCH,
        lat = -13.0,
        lon = -74.0,
        precisionM = 5.0,
        tipo = TipoAnomalia.Bache,
        severidad = 2,
        confidence = 0.9,
        modeloVersion = "1.0",
        seudonimo = "seu1",
        estado = EstadoLocal.PENDING,
        estadoDesde = Instant.EPOCH,
    )
}
