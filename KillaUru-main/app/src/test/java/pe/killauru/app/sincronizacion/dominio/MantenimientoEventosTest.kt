package pe.killauru.app.sincronizacion.dominio

import java.time.Duration
import java.time.Instant
import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.soporte.EventosMemoria
import kotlin.test.*

class MantenimientoEventosTest {
    @Test fun `expiracion exige hora confiable y respeta las 168 horas`() = runTest {
        val repo = EventosMemoria()
        var ahora: Instant? = null
        val reloj = object : Reloj {
            override fun ahoraConfiable() = ahora
            override fun monotonico() = Duration.ZERO
            override suspend fun anclarGps(utc: Instant, elapsed: Duration) = Unit
            override suspend fun anclarServidor(utc: Instant) = Unit
        }
        val caso = ExpirarPendientesUseCase(repo, reloj)
        caso.ejecutar()
        assertNull(repo.expiracion)
        ahora = Instant.parse("2026-01-08T00:00:00Z")
        caso.ejecutar()
        assertEquals(Instant.parse("2026-01-01T00:00:00Z").toEpochMilli(), repo.expiracion)
    }

    @Test fun `recupera envios interrumpidos y transmite el limite de purga`() = runTest {
        val repo = EventosMemoria()
        RecuperarEnviandoUseCase(repo).ejecutar()
        assertEquals(1, repo.recuperaciones)
        var limite: Long? = null
        val purgador = object : PurgadorEventos {
            override suspend fun purgarEventosFinales(anteriorAUtcMs: Long) { limite = anteriorAUtcMs }
        }
        PurgarLocalUseCase(purgador).ejecutar(1234)
        assertEquals(1234L, limite)
    }
}
