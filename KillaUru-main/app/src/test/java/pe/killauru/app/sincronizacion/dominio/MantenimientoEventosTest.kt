package pe.killauru.app.sincronizacion.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.soporte.EventosMemoria
import java.time.Duration
import java.time.Instant
import kotlin.test.assertEquals
import kotlin.test.assertNull

/** FR-023, FR-033, RN-17: expiración a las 168 h, recuperación de envíos cortados y purga. Patrón AAA. */
class MantenimientoEventosTest {
    private fun reloj(ahora: Instant?) = object : Reloj {
        override fun ahoraConfiable() = ahora
        override fun monotonico() = Duration.ZERO
        override suspend fun anclarGps(utc: Instant, elapsed: Duration) = Unit
        override suspend fun anclarServidor(utc: Instant) = Unit
    }

    @Test fun `sin hora confiable no expira nada`() = runTest {
        // Arrange — el reloj del celular puede estar mal: no se borra nada con él
        val repo = EventosMemoria()
        // Act
        ExpirarPendientesUseCase(repo, reloj(null)).ejecutar()
        // Assert
        assertNull(repo.expiracion)
    }

    @Test fun `con hora confiable expira lo pendiente de hace mas de 168 horas`() = runTest {
        // Arrange
        val repo = EventosMemoria()
        // Act
        ExpirarPendientesUseCase(repo, reloj(Instant.parse("2026-01-08T00:00:00Z"))).ejecutar()
        // Assert — 168 h = 7 días antes
        assertEquals(Instant.parse("2026-01-01T00:00:00Z").toEpochMilli(), repo.expiracion)
    }

    @Test fun `recupera los envios que quedaron cortados`() = runTest {
        // Arrange
        val repo = EventosMemoria()
        // Act
        RecuperarEnviandoUseCase(repo).ejecutar()
        // Assert
        assertEquals(1, repo.recuperaciones)
    }

    @Test fun `la purga recibe el limite indicado`() = runTest {
        // Arrange
        var limite: Long? = null
        val purgador = object : PurgadorEventos {
            override suspend fun purgarEventosFinales(anteriorAUtcMs: Long) { limite = anteriorAUtcMs }
        }
        // Act
        PurgarLocalUseCase(purgador).ejecutar(1234)
        // Assert
        assertEquals(1234L, limite)
    }
}
