package pe.killauru.app.mapa.datos

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import kotlinx.coroutines.test.runTest
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okio.Buffer
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import pe.killauru.app.mapa.dominio.ArchivoMapa
import pe.killauru.app.mapa.dominio.ManifiestoMapa
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import pe.killauru.app.soporte.PreferenciasMemoria
import java.security.MessageDigest
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

/** FR-043, FR-044: los archivos del mapa en la carpeta privada de la app. Robolectric y MockWebServer. Patrón AAA. */
@RunWith(RobolectricTestRunner::class)
class ArchivosMapaLocalTest {
    private lateinit var servidor: MockWebServer
    private lateinit var almacen: AlmacenArchivos
    private lateinit var prefs: PreferenciasMemoria
    private lateinit var archivos: ArchivosMapaLocal

    private fun manifiesto(version: String) = ManifiestoMapa(version, 0L, "© OSM", TipoArchivoMapa.entries.map {
        ArchivoMapa(it, version, "https://cdn/mapa/${it.name.lowercase()}.v$version.json", 3, "x")
    })

    @Before fun setUp() {
        servidor = MockWebServer().apply { start() }
        almacen = AlmacenArchivos(ApplicationProvider.getApplicationContext<Context>())
        prefs = PreferenciasMemoria()
        archivos = ArchivosMapaLocal(almacen, DescargaReanudable(CdnModulo.cdnApi(), almacen, VerificadorSha256()), prefs)
    }

    @After fun tearDown() {
        servidor.shutdown()
        almacen.vaciar()
    }

    @Test fun `sin mapa instalado no hay version ni etag`() = runTest {
        // Arrange + Act + Assert
        assertNull(archivos.instalado())
        assertNull(archivos.etag())
    }

    @Test fun `activar deja esa version, su etag y borra los archivos de la anterior`() = runTest {
        // Arrange — quedan archivos de la versión 2 y ya están los de la 3
        manifiesto("2").archivos.forEach { almacen.archivoMapa(it.nombre).writeText("v2") }
        manifiesto("3").archivos.forEach { almacen.archivoMapa(it.nombre).writeText("v3") }
        // Act
        archivos.activar(manifiesto("3"), "\"e3\"")
        // Assert
        assertEquals(manifiesto("3"), archivos.instalado())
        assertEquals("\"e3\"", archivos.etag())
        assertEquals("3", prefs.mapaVersion) // lo que muestra Ajustes
        assertTrue(manifiesto("3").archivos.all { almacen.archivoMapa(it.nombre).exists() })
        assertFalse(manifiesto("2").archivos.any { almacen.archivoMapa(it.nombre).exists() })
        assertEquals(almacen.archivoMapa("estilo.v3.json"), archivos.archivo(TipoArchivoMapa.ESTILO))
    }

    @Test fun `un estado ilegible cuenta como sin mapa en vez de romper la app`() = runTest {
        // Arrange
        almacen.estado().writeText("{roto")
        // Act + Assert
        assertNull(archivos.instalado())
    }

    @Test fun `descargar usa el tamano y la huella del manifiesto`() = runTest {
        // Arrange
        val contenido = "abc".toByteArray()
        val sha = MessageDigest.getInstance("SHA-256").digest(contenido).joinToString("") { "%02x".format(it) }
        val archivo = ArchivoMapa(TipoArchivoMapa.CORREDOR, "3", servidor.url("/mapa/corredor_pe28a.v3.json").toString(), 3, sha)
        servidor.enqueue(MockResponse().setBody(Buffer().write(contenido)))
        servidor.enqueue(MockResponse().setBody(Buffer().write(contenido)))
        // Act
        val bien = archivos.descargar(archivo) {}
        val mal = archivos.descargar(archivo.copy(bytes = 4)) {}
        // Assert
        assertTrue(bien)
        assertFalse(mal)
    }
}
