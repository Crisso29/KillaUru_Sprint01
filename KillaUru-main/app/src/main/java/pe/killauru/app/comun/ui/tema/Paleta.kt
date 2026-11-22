package pe.killauru.app.comun.ui.tema

import androidx.compose.material3.ColorScheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Immutable
import androidx.compose.ui.graphics.Color

/**
 * Colores del sistema de diseño de KillaUru (prototipo `referencia/prototipo/killauru-diseno.html`). Son los mismos que
 * el panel (`panel/src/compartido/estilos/tokens.css`) y `TemaCoherenteConPanelTest` lo comprueba. Cada propiedad
 * nombra su token CSS. `s2` y `s3` son más oscuros que en el prototipo: así llegan a 3:1 sobre blanco (WCAG 1.4.11),
 * igual que en el panel.
 */
@Immutable
internal data class Paleta(
    val fondo: Color, // --bg
    val superficie: Color, // --surface
    val hundido: Color, // --sunken
    val tinta: Color, // --ink
    val tinta2: Color, // --ink-2
    val tinta3: Color, // --ink-3
    val linea: Color, // --line
    val linea2: Color, // --line-2
    val killa: Color, // --killa: color de marca
    val sobreKilla: Color, // --killa-ink
    val killaSuave: Color, // --killa-soft
    val luna: Color, // --luna: acento
    val lunaSuave: Color, // --luna-soft
    val ok: Color, // --ok
    val okSuave: Color, // --ok-soft
    val aviso: Color, // --warn
    val avisoSuave: Color, // --warn-soft
    val mal: Color, // --bad
    val malSuave: Color, // --bad-soft
    val info: Color, // --info
    val infoSuave: Color, // --info-soft
    val severidades: List<Color>, // --s1 … --s5
    val sobreSeveridadBaja: Color, // --on-s13: texto sobre severidad 1 a 3
    val sobreSeveridadAlta: Color, // --on-s45: texto sobre severidad 4 y 5
) {
    /** Color de la severidad 1 a 5 (RN-02); fuera de rango usa el extremo más cercano. */
    fun colorSeveridad(severidad: Int): Color = severidades[indice(severidad)]

    /** Color del número sobre [colorSeveridad]: oscuro sobre 1 a 3, claro sobre 4 y 5 (contraste del prototipo). */
    fun textoSobreSeveridad(severidad: Int): Color =
        if (indice(severidad) < PRIMERA_ALTA) sobreSeveridadBaja else sobreSeveridadAlta

    private fun indice(severidad: Int) = (severidad - 1).coerceIn(severidades.indices)

    /** Traducción al esquema de Material 3, para que los componentes estándar ya salgan con los colores de KillaUru. */
    fun esquemaMaterial(oscuro: Boolean = false): ColorScheme {
        val base = if (oscuro) darkColorScheme() else lightColorScheme()
        return base.copy(
            primary = killa, onPrimary = sobreKilla, primaryContainer = killaSuave, onPrimaryContainer = tinta,
            secondary = tinta2, onSecondary = superficie,
            tertiary = luna, onTertiary = tinta, tertiaryContainer = lunaSuave, onTertiaryContainer = tinta,
            background = fondo, onBackground = tinta, surface = superficie, onSurface = tinta,
            surfaceVariant = hundido, onSurfaceVariant = tinta2, surfaceContainer = fondo,
            surfaceContainerLow = superficie, surfaceContainerHigh = hundido,
            outline = linea2, outlineVariant = linea,
            error = mal, onError = superficie, errorContainer = malSuave, onErrorContainer = tinta,
        )
    }

    companion object {
        private const val PRIMERA_ALTA = 3 // índice de la severidad 4

        val clara = Paleta(
            fondo = Color(0xFFF2F5F5), superficie = Color(0xFFFFFFFF), hundido = Color(0xFFE8EEEF),
            tinta = Color(0xFF0F1E24), tinta2 = Color(0xFF3D4F57), tinta3 = Color(0xFF63767E),
            linea = Color(0xFFDCE4E6), linea2 = Color(0xFFC3CFD2),
            killa = Color(0xFF0F5C70), sobreKilla = Color(0xFFFFFFFF), killaSuave = Color(0xFFDCEFEF),
            luna = Color(0xFFD4930F), lunaSuave = Color(0xFFFBF0D6),
            ok = Color(0xFF1F7F55), okSuave = Color(0xFFDFF2E8), aviso = Color(0xFFA66508), avisoSuave = Color(0xFFFCEFD6),
            mal = Color(0xFFC42839), malSuave = Color(0xFFFBE3E6), info = Color(0xFF2763A6), infoSuave = Color(0xFFE0ECF9),
            severidades = listOf(
                Color(0xFF4E8E77), Color(0xFF7E9A38), Color(0xFFB8820E), Color(0xFFE0642A), Color(0xFFC42839),
            ),
            sobreSeveridadBaja = Color(0xFF121829), sobreSeveridadAlta = Color(0xFFFFFFFF),
        )

        val oscura = Paleta(
            fondo = Color(0xFF08141A), superficie = Color(0xFF0F1E26), hundido = Color(0xFF0B181F),
            tinta = Color(0xFFE6F0F2), tinta2 = Color(0xFFA6BAC0), tinta3 = Color(0xFF7A9098),
            linea = Color(0xFF1E3440), linea2 = Color(0xFF2C4654),
            killa = Color(0xFF74D3C6), sobreKilla = Color(0xFF08141A), killaSuave = Color(0xFF12343A),
            luna = Color(0xFFF2C14E), lunaSuave = Color(0xFF3A2F17),
            ok = Color(0xFF4CC08A), okSuave = Color(0xFF12301F), aviso = Color(0xFFF0BA3A), avisoSuave = Color(0xFF382A0F),
            mal = Color(0xFFF25B6E), malSuave = Color(0xFF3B1820), info = Color(0xFF6EAAEA), infoSuave = Color(0xFF132748),
            severidades = listOf(
                Color(0xFF62B293), Color(0xFFA2C24E), Color(0xFFF0BA3A), Color(0xFFF57F45), Color(0xFFF25B6E),
            ),
            sobreSeveridadBaja = Color(0xFF08141A), sobreSeveridadAlta = Color(0xFF08141A),
        )
    }
}
