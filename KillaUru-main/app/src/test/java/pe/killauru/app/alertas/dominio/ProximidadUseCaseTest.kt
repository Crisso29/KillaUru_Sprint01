package pe.killauru.app.alertas.dominio

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/** T097: FR-054, FR-056 — ProximidadUseCase respeta zona de aviso por categoría. */
class ProximidadUseCaseTest {

    private val params = ParametrosAviso()
    private val uc = ProximidadUseCase(params)

    private fun anomalia(id: String, lat: Double, lon: Double) = AnomaliaLocal(
        id = id, tipo = TipoAnomalia.Bache, severidad = 2, persistente = false,
        coordenada = Coordenada(lat, lon), km = 10.0,
        venceUtc = null, ultimaDeteccionUtc = 0L, enDemo = true,
    )

    @Test
    fun `anomalia dentro de la zona genera alerta`() {
        // Arrange
        val pos = Coordenada(-13.5203, -74.1259)
        val cerca = anomalia("a1", -13.5204, -74.1259)
        // Act
        val alertas = uc.evaluar(pos, 60.0, CategoriaVehiculo.LIVIANO, listOf(cerca))
        // Assert
        assertEquals(1, alertas.size)
    }

    @Test
    fun `anomalia fuera de la zona no genera alerta`() {
        // Arrange
        val pos = Coordenada(-13.5203, -74.1259)
        val lejos = anomalia("a2", -13.6000, -74.1259)
        // Act
        val alertas = uc.evaluar(pos, 60.0, CategoriaVehiculo.LIVIANO, listOf(lejos))
        // Assert
        assertTrue(alertas.isEmpty())
    }

    @Test
    fun `pesado tiene mayor distancia de aviso que liviano`() {
        // Arrange
        val distLiviano = params.distanciaAviso(CategoriaVehiculo.LIVIANO, 80.0)
        val distPesado = params.distanciaAviso(CategoriaVehiculo.PESADO, 80.0)
        // Assert
        assertTrue(distPesado > distLiviano)
    }
}
