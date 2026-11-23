package pe.killauru.app.corredor.datos

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.mapa.datos.AlmacenArchivos
import pe.killauru.app.mapa.datos.ArchivosMapaLocal
import pe.killauru.app.mapa.datos.CdnModulo
import pe.killauru.app.mapa.datos.DescargaReanudable
import pe.killauru.app.mapa.datos.VerificadorSha256
import pe.killauru.app.mapa.dominio.ArchivoMapa
import pe.killauru.app.mapa.dominio.ManifiestoMapa
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import pe.killauru.app.soporte.PreferenciasMemoria
import pe.killauru.app.soporte.ValidadorContrato
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * FR-049, FR-055, FR-074: el corredor y el tramo demo se leen de los archivos descargados de la CDN (antes se leían de
 * `assets` que no existían). El corredor de prueba cumple el esquema `Corredor` del contrato. Robolectric. Patrón AAA.
 */
@RunWith(RobolectricTestRunner::class)
class CargadoresMapaTest {
    private lateinit var almacen: AlmacenArchivos
    private lateinit var archivos: ArchivosMapaLocal

    private val corredorJson = """{"version":"1","fuenteSha256":"${"a".repeat(64)}","radioTierraM":6371008.8,
        "vertices":[[-74.50,-13.33,268.0],[-74.40,-13.34,279.0],[-74.21,-13.15,335.073]]}"""
    private val demoJson = """{"type":"Feature","properties":{"vertices":4},"geometry":{"type":"Polygon",
        "coordinates":[[[-74.6,-13.4],[-74.1,-13.4],[-74.1,-13.1],[-74.6,-13.1],[-74.6,-13.4]]]}}"""

    private fun manifiesto() = ManifiestoMapa("1", 0L, "© OSM", listOf(
        ArchivoMapa(TipoArchivoMapa.CORREDOR, "1", "https://cdn/mapa/corredor_pe28a.v1.json", 1, "x"),
        ArchivoMapa(TipoArchivoMapa.TRAMO_DEMO, "1", "https://cdn/mapa/tramo_demo.v1.geojson", 1, "x"),
        ArchivoMapa(TipoArchivoMapa.PMTILES, "1", "https://cdn/mapa/corredor-pe28a.v1.pmtiles", 1, "x"),
        ArchivoMapa(TipoArchivoMapa.ESTILO, "1", "https://cdn/mapa/estilo.v1.json", 1, "x"),
    ))

    @Before fun setUp() {
        almacen = AlmacenArchivos(ApplicationProvider.getApplicationContext<Context>())
        archivos = ArchivosMapaLocal(almacen, DescargaReanudable(CdnModulo.cdnApi(), almacen, VerificadorSha256()), PreferenciasMemoria())
    }

    @After fun tearDown() = almacen.vaciar()

    private suspend fun instalar() {
        almacen.archivoMapa("corredor_pe28a.v1.json").writeText(corredorJson)
        almacen.archivoMapa("tramo_demo.v1.geojson").writeText(demoJson)
        archivos.activar(manifiesto(), null)
    }

    @Test fun `el corredor de prueba cumple el esquema Corredor del contrato`() {
        // Arrange + Act
        val errores = ValidadorContrato.validarJson(corredorJson,
            ValidadorContrato.esquema("008-app-android/contracts/cdn.openapi.yaml", "Corredor"))
        // Assert
        assertEquals(emptyList(), errores)
    }

    @Test fun `sin mapa descargado no hay corredor ni tramo demo y la app no se cae`() = runTest {
        // Arrange + Act + Assert
        assertNull(CargadorCorredor(archivos).corredor())
        assertNull(CargadorZonaDemo(archivos).zona())
    }

    @Test fun `el corredor descargado da el km de un punto sobre la via`() = runTest {
        // Arrange
        instalar()
        // Act
        val corredor = assertNotNull(CargadorCorredor(archivos).corredor())
        val pos = corredor.proyectar(Coordenada(-13.33, -74.50))
        // Assert — vértices [lon, lat, km] del contrato
        assertEquals(3, corredor.vertices.size)
        assertEquals(268.0, assertNotNull(pos).km, 0.01)
    }

    @Test fun `el tramo demo descargado sabe que esta dentro y que fuera`() = runTest {
        // Arrange
        instalar()
        // Act
        val zona = assertNotNull(CargadorZonaDemo(archivos).zona())
        // Assert
        assertTrue(zona.contiene(Coordenada(-13.25, -74.3)))
        assertFalse(zona.contiene(Coordenada(-13.7, -75.5)))
    }
}
