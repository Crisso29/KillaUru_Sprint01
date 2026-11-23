package pe.killauru.app.arquitectura

import org.junit.Test
import java.io.File
import javax.xml.parsers.DocumentBuilderFactory
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * RNF de seguridad (HTTPS siempre en producción): solo la variante de depuración puede hablar `http`, y únicamente con
 * el equipo del desarrollador visto desde el emulador; la versión publicada no permite texto plano y su URL es HTTPS.
 * Lee los archivos reales del módulo. Patrón AAA.
 */
class RedSeguraTest {
    private val raiz = File(".").let { if (File(it, "src/main").isDirectory) it else File(it, "app") }

    @Test fun `la version publicada no permite trafico en claro`() {
        // Arrange + Act
        val manifiesto = File(raiz, "src/main/AndroidManifest.xml").readText()
        // Assert
        assertTrue("android:usesCleartextTraffic=\"false\"" in manifiesto)
        assertTrue("networkSecurityConfig" !in manifiesto, "la excepción de red solo puede estar en src/debug")
    }

    @Test fun `depuracion solo permite http hacia el equipo del desarrollador`() {
        // Arrange
        val xml = DocumentBuilderFactory.newInstance().newDocumentBuilder()
            .parse(File(raiz, "src/debug/res/xml/seguridad_red_depuracion.xml"))
        // Act
        val base = xml.getElementsByTagName("base-config").item(0).attributes.getNamedItem("cleartextTrafficPermitted").nodeValue
        val dominios = xml.getElementsByTagName("domain").let { l -> (0 until l.length).map { l.item(it).textContent.trim() } }
        // Assert
        assertEquals("false", base)
        assertEquals(setOf("10.0.2.2", "localhost"), dominios.toSet())
    }

    @Test fun `las url de la api y de la cdn publicadas son https y fijas`() {
        // Arrange + Act
        val gradle = File(raiz, "build.gradle.kts").readText()
        val release = gradle.substringAfter("release {").substringBefore("}")
        // Assert — release no lee propiedades: nadie puede publicar un APK que apunte a otro servidor
        assertTrue("\"\\\"https://api.killauru.pe/\\\"\"" in release, release)
        assertTrue("\"\\\"https://cdn.killauru.pe/\\\"\"" in release, release)
        assertTrue("findProperty" !in release)
    }
}
