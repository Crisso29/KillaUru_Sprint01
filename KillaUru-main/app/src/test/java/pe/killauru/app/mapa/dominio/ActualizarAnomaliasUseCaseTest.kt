package pe.killauru.app.mapa.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

/**
 * T082 / FR-044, FR-045, FR-052: lógica de delta, ETag, 304, 400 y limpieza de sesión.
 * Patrón AAA en cada test.
 */
class ActualizarAnomaliasUseCaseTest {

    // ── dobles de prueba ─────────────────────────────────────────────────────

    private fun anomaliaEjemplo(id: String = "a1", enDemo: Boolean = false) = AnomaliaLocal(
        id = id,
        tipo = TipoAnomalia.Bache,
        severidad = 3,
        persistente = false,
        coordenada = Coordenada(-13.5, -74.5),
        km = 290.0,
        venceUtc = 9_999_999L,
        ultimaDeteccionUtc = 1_000L,
        enDemo = enDemo,
    )

    // ── tests ────────────────────────────────────────────────────────────────

    @Test fun `registrado sin copia descarga todo sin updatedSince`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria()
        val fuente = FuenteAnomaliasMemoria(
            FuenteAnomalias.Respuesta.Datos(
                altas = listOf(anomaliaEjemplo("x")),
                bajas = emptyList(),
                generadoUtc = 5000L,
                etag = null,
            )
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertNull(fuente.ultimoUpdatedSince, "no debe enviar updatedSince en descarga completa")
        assertEquals(1, repo.anomalias.size)
    }

    @Test fun `registrado con copia envia updatedSince igual al generadoUtc previo`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria().also {
            it.estado = EstadoCopia(Alcance.Registrado, generadoUtc = 3000L, etag = null, verificadoUtc = null)
        }
        val fuente = FuenteAnomaliasMemoria(
            FuenteAnomalias.Respuesta.Datos(emptyList(), emptyList(), 6000L, null)
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(3000L, fuente.ultimoUpdatedSince)
    }

    @Test fun `bajas se eliminan de la copia local`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria().also {
            it.anomalias += anomaliaEjemplo("del")
            it.anomalias += anomaliaEjemplo("keep")
            it.estado = EstadoCopia(Alcance.Registrado, 1000L, null, null)
        }
        val fuente = FuenteAnomaliasMemoria(
            FuenteAnomalias.Respuesta.Datos(emptyList(), listOf("del"), 2000L, null)
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(1, repo.anomalias.size)
        assertEquals("keep", repo.anomalias.first().id)
    }

    @Test fun `anonimo pide completo con If-None-Match del etag previo`() = runTest {
        // Arrange
        val etag = "abc123"
        val repo = RepositorioAnomaliasMemoria().also {
            it.estado = EstadoCopia(Alcance.Anonimo, null, etag, null)
        }
        val fuente = FuenteAnomaliasMemoria(FuenteAnomalias.Respuesta.SinCambios(999L))
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Anonimo)
        // Assert
        assertNull(fuente.ultimoUpdatedSince, "anónimo no envía updatedSince")
        assertEquals(etag, fuente.ultimoEtag)
    }

    @Test fun `304 conserva anomalias y actualiza verificadoUtc`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria().also {
            it.anomalias += anomaliaEjemplo("original")
            it.estado = EstadoCopia(Alcance.Anonimo, null, "etag", 100L)
        }
        val verificado = 500L
        val fuente = FuenteAnomaliasMemoria(FuenteAnomalias.Respuesta.SinCambios(verificado))
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        val resultado = caso.actualizar(Alcance.Anonimo)
        // Assert
        assertIs<ActualizarAnomaliasUseCase.Resultado.SinCambios>(resultado)
        assertEquals(1, repo.anomalias.size, "no debe tocar las anomalías")
        assertEquals(verificado, repo.estado?.verificadoUtc)
    }

    /**
     * FR-026: si el 304 llega sin la hora del servidor (`Date`), no se inventa una con el reloj del celular:
     * se conserva la última marca conocida.
     */
    @Test fun `304 sin hora del servidor conserva el verificadoUtc anterior`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria().also {
            it.estado = EstadoCopia(Alcance.Anonimo, null, "etag", 100L)
        }
        val fuente = FuenteAnomaliasMemoria(FuenteAnomalias.Respuesta.SinCambios(verificadoUtc = null))
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Anonimo)
        // Assert
        assertEquals(100L, repo.estado?.verificadoUtc)
    }

    /**
     * data-model §estado_copia y FR-026: «Actualizado hace…» usa la hora del **servidor** (`generadoUtc` del 200),
     * nunca el reloj del celular, que puede estar mal o no ser confiable.
     */
    @Test fun `200 guarda como verificadoUtc la hora del servidor y no la del celular`() = runTest {
        // Arrange — el servidor generó la respuesta en 5 000 ms (el caso de uso no tiene acceso al reloj del celular)
        val repo = RepositorioAnomaliasMemoria()
        val fuente = FuenteAnomaliasMemoria(
            FuenteAnomalias.Respuesta.Datos(emptyList(), emptyList(), generadoUtc = 5000L, etag = null)
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(5000L, repo.estado?.verificadoUtc)
    }

    @Test fun `400 descarta el delta y repite descarga completa`() = runTest {
        // Arrange
        var llamadas = 0
        val repo = RepositorioAnomaliasMemoria().also {
            it.estado = EstadoCopia(Alcance.Registrado, 2000L, null, null)
        }
        // Primera llamada → 400; segunda (sin updatedSince) → datos
        val fuente = object : FuenteAnomalias {
            override suspend fun obtener(alcance: Alcance, updatedSince: Long?, etag: String?): FuenteAnomalias.Respuesta {
                llamadas++
                return if (llamadas == 1) FuenteAnomalias.Respuesta.SolicitudInvalida
                else FuenteAnomalias.Respuesta.Datos(listOf(anomaliaEjemplo("nuevo")), emptyList(), 9000L, null)
            }
        }
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(2, llamadas, "debe hacer exactamente 2 llamadas")
        assertEquals(1, repo.anomalias.size)
    }

    @Test fun `al limpiar cambia de rol borra toda la copia`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria().also {
            it.anomalias += anomaliaEjemplo("x")
            it.estado = EstadoCopia(Alcance.Registrado, 1000L, null, null)
        }
        val fuente = FuenteAnomaliasMemoria(FuenteAnomalias.Respuesta.SinCambios(0))
        val caso = ActualizarAnomaliasUseCase(repo, fuente)
        // Act
        caso.limpiarAlCambiarRol()
        // Assert
        assertEquals(1, repo.limpiezas)
        assertEquals(0, repo.anomalias.size)
    }
}
