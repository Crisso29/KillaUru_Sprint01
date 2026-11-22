package pe.killauru.app.comun.ui.tema

import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import org.junit.Test
import java.io.File
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * Sistema de diseño (prototipo `referencia/prototipo/killauru-diseno.html`): la app y el panel usan la misma paleta.
 * La fuente única es `panel/src/compartido/estilos/tokens.css`, que el panel ya prueba con WCAG; esta prueba la lee y
 * exige que cada color de la app sea idéntico, en claro y en oscuro. Patrón AAA.
 */
class TemaCoherenteConPanelTest {
    private val tokensCss = File("../panel/src/compartido/estilos/tokens.css")

    /** Primer bloque `:root{}` (claro) y bloque de `prefers-color-scheme: dark` (oscuro). */
    private fun bloques(): Pair<Map<String, String>, Map<String, String>> {
        val css = tokensCss.readText()
        val claro = Regex(""":root\s*\{(.*?)\}""", RegexOption.DOT_MATCHES_ALL).find(css)!!.groupValues[1]
        val oscuro = Regex("""prefers-color-scheme:\s*dark\)\s*\{[^{]*\{(.*?)\}""", RegexOption.DOT_MATCHES_ALL)
            .find(css)!!.groupValues[1]
        return colores(claro) to colores(oscuro)
    }

    private fun colores(bloque: String) = Regex("""(--[\w-]+)\s*:\s*(#[0-9A-Fa-f]{6})\s*;""")
        .findAll(bloque).associate { it.groupValues[1] to it.groupValues[2].uppercase() }

    private fun hex(c: Color) = "#%06X".format(c.toArgb() and 0xFFFFFF)

    /** Token CSS → color de la app con el que debe coincidir. */
    private fun mapa(p: Paleta) = mapOf(
        "--bg" to p.fondo, "--surface" to p.superficie, "--sunken" to p.hundido,
        "--ink" to p.tinta, "--ink-2" to p.tinta2, "--ink-3" to p.tinta3, "--line" to p.linea, "--line-2" to p.linea2,
        "--killa" to p.killa, "--killa-ink" to p.sobreKilla, "--killa-soft" to p.killaSuave,
        "--luna" to p.luna, "--luna-soft" to p.lunaSuave,
        "--ok" to p.ok, "--ok-soft" to p.okSuave, "--warn" to p.aviso, "--warn-soft" to p.avisoSuave,
        "--bad" to p.mal, "--bad-soft" to p.malSuave, "--info" to p.info, "--info-soft" to p.infoSuave,
        "--s1" to p.severidades[0], "--s2" to p.severidades[1], "--s3" to p.severidades[2],
        "--s4" to p.severidades[3], "--s5" to p.severidades[4],
        "--on-s13" to p.sobreSeveridadBaja, "--on-s45" to p.sobreSeveridadAlta,
    )

    @Test fun `la paleta clara de la app es la del panel`() {
        // Arrange
        val (claro, _) = bloques()
        // Act
        val distintos = mapa(Paleta.clara).filter { (token, color) -> claro[token] != hex(color) }.keys
        // Assert
        assertTrue(distintos.isEmpty(), "Distintos del panel en claro: $distintos")
    }

    @Test fun `la paleta oscura de la app es la del panel`() {
        // Arrange
        val (_, oscuro) = bloques()
        // Act
        val distintos = mapa(Paleta.oscura).filter { (token, color) -> oscuro[token] != hex(color) }.keys
        // Assert
        assertTrue(distintos.isEmpty(), "Distintos del panel en oscuro: $distintos")
    }

    /** El tema de ventana (lo que se ve antes de que cargue Compose) no puede quedarse con otros colores. */
    @Test fun `los colores de la ventana son los de la paleta en claro y oscuro`() {
        // Arrange
        fun xml(carpeta: String) = Regex("""<color name="(\w+)">(#[0-9A-F]{6})</color>""")
            .findAll(File("src/main/res/$carpeta/colors.xml").readText()).associate { it.groupValues[1] to it.groupValues[2] }
        // Act
        val claro = xml("values")
        val oscuro = xml("values-night")
        // Assert
        assertEquals(mapOf("killa_fondo" to hex(Paleta.clara.fondo), "killa_marca" to hex(Paleta.clara.killa)), claro)
        assertEquals(mapOf("killa_fondo" to hex(Paleta.oscura.fondo), "killa_marca" to hex(Paleta.oscura.killa)), oscuro)
    }

    @Test fun `el color de marca de material es killa`() {
        // Arrange + Act
        val esquema = Paleta.clara.esquemaMaterial()
        // Assert
        assertEquals(Paleta.clara.killa, esquema.primary)
        assertEquals(Paleta.clara.fondo, esquema.background)
        assertEquals(Paleta.clara.mal, esquema.error)
    }
}
