package pe.killauru.app.mapa.datos

import android.content.Context
import androidx.room.Room
import androidx.test.core.app.ApplicationProvider
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import pe.killauru.app.comun.datos.bd.KillaUruBd
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.mapa.dominio.EstadoCopia
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertNull

/**
 * T084 / FR-044, FR-048, FR-051: contrato de persistencia Room.
 * Patrón AAA en cada test. Robolectric + BD en memoria.
 */
@RunWith(RobolectricTestRunner::class)
class RepositorioAnomaliasRoomTest {

    private lateinit var bd: KillaUruBd
    private lateinit var repo: RepositorioAnomaliasRoom

    @Before fun setUp() {
        val ctx = ApplicationProvider.getApplicationContext<Context>()
        bd = Room.inMemoryDatabaseBuilder(ctx, KillaUruBd::class.java)
            .allowMainThreadQueries()
            .build()
        repo = RepositorioAnomaliasRoom(bd, bd.anomaliaDao(), bd.estadoCopiaDao())
    }

    @After fun tearDown() {
        bd.close()
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private fun anomalia(
        id: String,
        enDemo: Boolean = false,
        persistente: Boolean = false,
        venceUtc: Long? = Long.MAX_VALUE,
    ) = AnomaliaLocal(
        id = id,
        tipo = TipoAnomalia.Bache,
        severidad = 2,
        persistente = persistente,
        coordenada = Coordenada(-13.5, -74.5),
        km = 290.0,
        venceUtc = venceUtc,
        ultimaDeteccionUtc = 1_000L,
        enDemo = enDemo,
    )

    private fun estado(alcance: Alcance = Alcance.Registrado, generadoUtc: Long? = 5000L) =
        EstadoCopia(alcance, generadoUtc, etag = null, verificadoUtc = null)

    // ── tests ────────────────────────────────────────────────────────────────

    @Test fun `actualizarLote guarda altas y estado en la misma transaccion`() = runTest {
        // Arrange
        val altas = listOf(anomalia("a1"), anomalia("a2"))
        val est = estado()
        // Act
        repo.actualizarLote(altas, emptyList(), est)
        // Assert
        assertEquals(2, repo.vigentes(0L).size)
        assertNotNull(repo.obtenerEstado())
        assertEquals(5000L, repo.obtenerEstado()?.generadoUtc)
    }

    @Test fun `actualizarLote elimina bajas indicadas`() = runTest {
        // Arrange
        repo.actualizarLote(listOf(anomalia("a1"), anomalia("a2")), emptyList(), estado())
        // Act
        repo.actualizarLote(emptyList(), listOf("a1"), estado(generadoUtc = 6000L))
        // Assert
        val vigentes = repo.vigentes(0L)
        assertEquals(1, vigentes.size)
        assertEquals("a2", vigentes.first().id)
    }

    @Test fun `vigentes filtra por venceUtc mayor que ahora`() = runTest {
        // Arrange
        val expirada = anomalia("exp", venceUtc = 100L)
        val activa = anomalia("act", venceUtc = Long.MAX_VALUE)
        repo.actualizarLote(listOf(expirada, activa), emptyList(), estado())
        // Act
        val resultado = repo.vigentes(ahoraMs = 200L)
        // Assert
        assertEquals(1, resultado.size)
        assertEquals("act", resultado.first().id)
    }

    @Test fun `persistente permanece vigente sin importar el tiempo`() = runTest {
        // Arrange
        val fija = anomalia("fija", persistente = true, venceUtc = null)
        repo.actualizarLote(listOf(fija), emptyList(), estado())
        // Act - muy en el futuro
        val resultado = repo.vigentes(ahoraMs = Long.MAX_VALUE - 1)
        // Assert
        assertEquals(1, resultado.size)
    }

    @Test fun `recortarAlDemo elimina anomalias fuera del demo`() = runTest {
        // Arrange
        val enDemo = anomalia("d1", enDemo = true)
        val fueraDemo = anomalia("f1", enDemo = false)
        repo.actualizarLote(listOf(enDemo, fueraDemo), emptyList(), estado())
        // Act
        repo.recortarAlDemo()
        // Assert
        assertEquals(1, repo.vigentes(0L).size)
        assertEquals("d1", repo.vigentes(0L).first().id)
    }

    @Test fun `limpiarCopia borra anomalias y estado`() = runTest {
        // Arrange
        repo.actualizarLote(listOf(anomalia("x")), emptyList(), estado())
        // Act
        repo.limpiarCopia()
        // Assert
        assertEquals(0, repo.vigentes(0L).size)
        assertNull(repo.obtenerEstado())
    }

    @Test fun `obtenerEstado devuelve null cuando la copia esta vacia`() = runTest {
        // Arrange - BD vacía
        // Act + Assert
        assertNull(repo.obtenerEstado())
    }
}
