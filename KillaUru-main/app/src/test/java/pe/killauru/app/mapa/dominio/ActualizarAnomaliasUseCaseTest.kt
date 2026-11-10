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

    private class RepoMemoria : RepositorioAnomalias {
        val anomalias = mutableListOf<AnomaliaLocal>()
        var estado: EstadoCopia? = null
        var limpiezas = 0

        override suspend fun vigentes(ahoraMs: Long) = anomalias.toList()

        override suspend fun actualizarLote(
            altas: List<AnomaliaLocal>,
            bajas: List<String>,
            estado: EstadoCopia,
        ) {
            anomalias.removeAll { it.id in bajas }
            anomalias.addAll(altas)
            this.estado = estado
        }

        override suspend fun recortarAlDemo() {
            anomalias.removeAll { !it.enDemo }
        }

        override suspend fun obtenerEstado() = estado
        override suspend fun limpiarCopia() {
            anomalias.clear()
            estado = null
            limpiezas++
        }
    }

    private class FuenteMemoria(
        var respuesta: FuenteAnomalias.Respuesta,
    ) : FuenteAnomalias {
        var llamadas = 0
        var ultimoUpdatedSince: Long? = null
        var ultimoEtag: String? = null

        override suspend fun obtener(
            alcance: Alcance,
            updatedSince: Long?,
            etag: String?,
        ): FuenteAnomalias.Respuesta {
            llamadas++
            ultimoUpdatedSince = updatedSince
            ultimoEtag = etag
            return respuesta
        }
    }

    private class RelojFijo(private val ahoraMs: Long = 1_000_000L) :
        pe.killauru.app.comun.dominio.Reloj {
        override fun ahoraMs() = ahoraMs
    }

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
        val repo = RepoMemoria()
        val fuente = FuenteMemoria(
            FuenteAnomalias.Respuesta.Datos(
                altas = listOf(anomaliaEjemplo("x")),
                bajas = emptyList(),
                generadoUtc = 5000L,
                etag = null,
            )
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertNull(fuente.ultimoUpdatedSince, "no debe enviar updatedSince en descarga completa")
        assertEquals(1, repo.anomalias.size)
    }

    @Test fun `registrado con copia envia updatedSince igual al generadoUtc previo`() = runTest {
        // Arrange
        val repo = RepoMemoria().also {
            it.estado = EstadoCopia(Alcance.Registrado, generadoUtc = 3000L, etag = null, verificadoUtc = null)
        }
        val fuente = FuenteMemoria(
            FuenteAnomalias.Respuesta.Datos(emptyList(), emptyList(), 6000L, null)
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(3000L, fuente.ultimoUpdatedSince)
    }

    @Test fun `bajas se eliminan de la copia local`() = runTest {
        // Arrange
        val repo = RepoMemoria().also {
            it.anomalias += anomaliaEjemplo("del")
            it.anomalias += anomaliaEjemplo("keep")
            it.estado = EstadoCopia(Alcance.Registrado, 1000L, null, null)
        }
        val fuente = FuenteMemoria(
            FuenteAnomalias.Respuesta.Datos(emptyList(), listOf("del"), 2000L, null)
        )
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(1, repo.anomalias.size)
        assertEquals("keep", repo.anomalias.first().id)
    }

    @Test fun `anonimo pide completo con If-None-Match del etag previo`() = runTest {
        // Arrange
        val etag = "abc123"
        val repo = RepoMemoria().also {
            it.estado = EstadoCopia(Alcance.Anonimo, null, etag, null)
        }
        val fuente = FuenteMemoria(FuenteAnomalias.Respuesta.SinCambios(999L))
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        caso.actualizar(Alcance.Anonimo)
        // Assert
        assertNull(fuente.ultimoUpdatedSince, "anónimo no envía updatedSince")
        assertEquals(etag, fuente.ultimoEtag)
    }

    @Test fun `304 conserva anomalias y actualiza verificadoUtc`() = runTest {
        // Arrange
        val repo = RepoMemoria().also {
            it.anomalias += anomaliaEjemplo("original")
            it.estado = EstadoCopia(Alcance.Anonimo, null, "etag", 100L)
        }
        val verificado = 500L
        val fuente = FuenteMemoria(FuenteAnomalias.Respuesta.SinCambios(verificado))
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        val resultado = caso.actualizar(Alcance.Anonimo)
        // Assert
        assertIs<ActualizarAnomaliasUseCase.Resultado.SinCambios>(resultado)
        assertEquals(1, repo.anomalias.size, "no debe tocar las anomalías")
        assertEquals(verificado, repo.estado?.verificadoUtc)
    }

    @Test fun `400 descarta el delta y repite descarga completa`() = runTest {
        // Arrange
        var llamadas = 0
        val repo = RepoMemoria().also {
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
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        caso.actualizar(Alcance.Registrado)
        // Assert
        assertEquals(2, llamadas, "debe hacer exactamente 2 llamadas")
        assertEquals(1, repo.anomalias.size)
    }

    @Test fun `al limpiar cambia de rol borra toda la copia`() = runTest {
        // Arrange
        val repo = RepoMemoria().also {
            it.anomalias += anomaliaEjemplo("x")
            it.estado = EstadoCopia(Alcance.Registrado, 1000L, null, null)
        }
        val fuente = FuenteMemoria(FuenteAnomalias.Respuesta.SinCambios(0))
        val caso = ActualizarAnomaliasUseCase(repo, fuente, RelojFijo())
        // Act
        caso.limpiarAlCambiarRol()
        // Assert
        assertEquals(1, repo.limpiezas)
        assertEquals(0, repo.anomalias.size)
    }
}
