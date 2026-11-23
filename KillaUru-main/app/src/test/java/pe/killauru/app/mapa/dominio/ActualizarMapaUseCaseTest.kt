package pe.killauru.app.mapa.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull
import kotlin.test.assertTrue

/**
 * T087, T089 / FR-041…FR-044, RNF-15: cuándo se descarga el mapa y que solo se activa si llegan los 4 archivos
 * íntegros (si no, sigue el mapa anterior). Dobles en memoria de la CDN y del disco. Patrón AAA.
 */
class ActualizarMapaUseCaseTest {
    private fun manifiesto(version: String) = ManifiestoMapa(version, 0L, "© OSM", TipoArchivoMapa.entries.map {
        ArchivoMapa(it, version, "https://cdn/mapa/${it.name.lowercase()}.v$version", bytes = 100, sha256 = "x")
    })

    private class CdnDoble(var respuesta: RespuestaManifiesto) : CdnMapa {
        var etagRecibido: String? = null
        override suspend fun manifiesto(etagPrevio: String?) = respuesta.also { etagRecibido = etagPrevio }
    }

    private class ArchivosDoble(var instalado: ManifiestoMapa? = null, var etagGuardado: String? = null) : ArchivosMapa {
        val descargados = mutableListOf<TipoArchivoMapa>()
        var fallaEn: TipoArchivoMapa? = null
        override suspend fun instalado() = instalado
        override suspend fun etag() = etagGuardado
        override suspend fun descargar(archivo: ArchivoMapa, alProgresar: (Long) -> Unit): Boolean {
            if (archivo.tipo == fallaEn) return false
            alProgresar(archivo.bytes)
            descargados += archivo.tipo
            return true
        }
        override suspend fun activar(manifiesto: ManifiestoMapa, etag: String?) { instalado = manifiesto; etagGuardado = etag }
    }

    private fun caso(cdn: CdnMapa, archivos: ArchivosMapa, moviles: Boolean = false) =
        ActualizarMapaUseCase(cdn, archivos, PreferenciasMemoria().apply { usarDatosMoviles = moviles }, PoliticaDescarga())

    @Test fun `sin mapa y con wifi descarga los 4 archivos, avisa el progreso y activa la version`() = runTest {
        // Arrange
        val archivos = ArchivosDoble()
        val progreso = mutableListOf<Int>()
        // Act
        val r = caso(CdnDoble(RespuestaManifiesto.Nuevo(manifiesto("3"), "e3")), archivos).ejecutar(hayWifi = true) { p, _ -> progreso += p }
        // Assert
        assertEquals(ResultadoMapa.Instalado("3"), r)
        assertEquals(TipoArchivoMapa.entries.toList(), archivos.descargados.toList())
        assertEquals("3", archivos.instalado?.version)
        assertEquals("e3", archivos.etagGuardado)
        assertEquals(100, progreso.last())
    }

    @Test fun `si un archivo falla no se activa nada y sigue el mapa anterior`() = runTest {
        // Arrange
        val archivos = ArchivosDoble(instalado = manifiesto("2")).apply { fallaEn = TipoArchivoMapa.ESTILO }
        // Act
        val r = caso(CdnDoble(RespuestaManifiesto.Nuevo(manifiesto("3"), null)), archivos).ejecutar(hayWifi = true) { _, _ -> }
        // Assert — lo descargado queda como parcial para reanudar la próxima vez
        assertIs<ResultadoMapa.Fallo>(r)
        assertEquals("2", archivos.instalado?.version)
    }

    @Test fun `con el etag guardado y un 304 el mapa esta al dia sin descargar`() = runTest {
        // Arrange
        val cdn = CdnDoble(RespuestaManifiesto.SinCambios)
        val archivos = ArchivosDoble(instalado = manifiesto("3"), etagGuardado = "e3")
        // Act
        val r = caso(cdn, archivos).ejecutar(hayWifi = true) { _, _ -> }
        // Assert
        assertEquals(ResultadoMapa.AlDia, r)
        assertEquals("e3", cdn.etagRecibido)
        assertTrue(archivos.descargados.isEmpty())
    }

    @Test fun `la misma version ya instalada no se vuelve a bajar`() = runTest {
        // Arrange
        val archivos = ArchivosDoble(instalado = manifiesto("3"))
        // Act
        val r = caso(CdnDoble(RespuestaManifiesto.Nuevo(manifiesto("3"), "e3")), archivos).ejecutar(hayWifi = true) { _, _ -> }
        // Assert — guarda el ETag para que la próxima vez sea un 304
        assertEquals(ResultadoMapa.AlDia, r)
        assertEquals("e3", archivos.etagGuardado)
        assertTrue(archivos.descargados.isEmpty())
    }

    @Test fun `con datos moviles sin permiso espera el wifi y dice cuanto pesa`() = runTest {
        // Arrange
        val archivos = ArchivosDoble()
        // Act
        val r = caso(CdnDoble(RespuestaManifiesto.Nuevo(manifiesto("3"), null)), archivos, moviles = false).ejecutar(hayWifi = false) { _, _ -> }
        // Assert — RNF-15: nunca gasta datos móviles sin permiso
        assertEquals(ResultadoMapa.EsperandoWifi(bytes = 400), r)
        assertTrue(archivos.descargados.isEmpty())
        assertNull(archivos.instalado)
    }

    @Test fun `con permiso para datos moviles descarga sin wifi`() = runTest {
        // Arrange + Act
        val r = caso(CdnDoble(RespuestaManifiesto.Nuevo(manifiesto("3"), null)), ArchivosDoble(), moviles = true).ejecutar(hayWifi = false) { _, _ -> }
        // Assert
        assertEquals(ResultadoMapa.Instalado("3"), r)
    }

    @Test fun `sin red el resultado es Fallo para reintentar`() = runTest {
        // Arrange + Act
        val r = caso(CdnDoble(RespuestaManifiesto.Fallo(java.io.IOException("sin red"))), ArchivosDoble()).ejecutar(hayWifi = true) { _, _ -> }
        // Assert
        assertIs<ResultadoMapa.Fallo>(r)
    }
}
