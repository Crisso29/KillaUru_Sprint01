package pe.killauru.app.mapa.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * FR-051, RN-08, data-model §anomalia: al guardar, `enDemo` se calcula con el polígono descargado (el contrato no lo
 * trae). Es lo que conserva el recorte al cerrar sesión. Patrón AAA.
 */
class EnDemoAnomaliasTest {
    private fun anomaliaEjemplo(id: String) = AnomaliaLocal(id, TipoAnomalia.Bache, 3, false, Coordenada(-13.5, -74.5), 290.0,
        9_999_999L, 1_000L, enDemo = false)

    private val sinZona = object : FuenteZonaDemo { override suspend fun zona(): ZonaDemo? = null }

    /** Cuadrado alrededor de (-13.5, -74.5): ahí está [anomaliaEjemplo]. */
    private val zonaAlrededor = object : FuenteZonaDemo {
        override suspend fun zona() = ZonaDemo(listOf(Coordenada(-13.6, -74.6), Coordenada(-13.6, -74.4),
            Coordenada(-13.4, -74.4), Coordenada(-13.4, -74.6)))
    }

    private fun datos(vararg altas: AnomaliaLocal) = FuenteAnomaliasMemoria(FuenteAnomalias.Respuesta.Datos(altas.toList(), emptyList(), 5000L, null))

    /** FR-051, data-model: `enDemo` se calcula con el polígono descargado; antes quedaba siempre en false. */
    @Test fun `al guardar marca en el demo solo lo que cae dentro del poligono`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria()
        val fuera = anomaliaEjemplo("fuera").copy(coordenada = Coordenada(-13.0, -75.9))
        // Act
        ActualizarAnomaliasUseCase(repo, datos(anomaliaEjemplo("dentro"), fuera), zonaAlrededor).actualizar(Alcance.Registrado)
        // Assert
        assertEquals(mapOf("dentro" to true, "fuera" to false), repo.anomalias.associate { it.id to it.enDemo })
    }

    /** RN-08, R-012: el servidor ya recorta lo del anónimo; sin polígono descargado igual se conserva al recortar. */
    @Test fun `lo que recibe el anonimo es del demo aunque aun no haya poligono`() = runTest {
        // Arrange
        val repo = RepositorioAnomaliasMemoria()
        // Act
        ActualizarAnomaliasUseCase(repo, datos(anomaliaEjemplo("a")), sinZona).actualizar(Alcance.Anonimo)
        // Assert
        assertTrue(repo.anomalias.single().enDemo)
    }
}
