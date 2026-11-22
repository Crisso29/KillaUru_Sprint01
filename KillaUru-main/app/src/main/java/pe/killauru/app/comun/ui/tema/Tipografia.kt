package pe.killauru.app.comun.ui.tema

import androidx.compose.material3.Typography
import androidx.compose.ui.text.ExperimentalTextApi
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontVariation
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp
import pe.killauru.app.R

/**
 * Tipografía del prototipo: Overpass para títulos (`--f-display`), Source Sans 3 para el texto (`--f-body`) y Overpass
 * Mono para datos (`--f-data`). Escala φ: 13 · 16 · 21 · 26 · 34 · 55. Las fuentes van en el APK (licencia OFL en
 * `assets/licencias/`) porque las descargables de Google exigen Play Services, que la app no usa (FR-009).
 */
// Una sola fuente variable por familia (cada peso es un eje, no otro archivo): la API de ejes aún es experimental.
@OptIn(ExperimentalTextApi::class)
private fun variable(recurso: Int, vararg pesos: Int) = FontFamily(
    pesos.map { peso ->
        Font(recurso, FontWeight(peso), variationSettings = FontVariation.Settings(FontVariation.weight(peso)))
    },
)

internal val FuenteTitulos = variable(R.font.overpass, 600, 700, 800, 900)
internal val FuenteTexto = variable(R.font.source_sans_3, 400, 600, 700)
internal val FuenteDatos = variable(R.font.overpass_mono, 400, 600)

private fun titulo(tamano: Int, peso: Int) =
    TextStyle(fontFamily = FuenteTitulos, fontWeight = FontWeight(peso), fontSize = tamano.sp, lineHeight = (tamano * 1.12).sp)

private fun texto(tamano: Int, peso: Int = 400) =
    TextStyle(fontFamily = FuenteTexto, fontWeight = FontWeight(peso), fontSize = tamano.sp, lineHeight = (tamano * 1.38).sp)

internal val TipografiaKillaUru = Typography(
    displayLarge = titulo(55, 900),
    displayMedium = titulo(34, 900),
    displaySmall = titulo(26, 900),
    headlineLarge = titulo(34, 800),
    headlineMedium = titulo(26, 800),
    headlineSmall = titulo(21, 800),
    titleLarge = titulo(21, 700),
    titleMedium = titulo(16, 700),
    titleSmall = titulo(13, 700),
    bodyLarge = texto(16),
    bodyMedium = texto(16),
    bodySmall = texto(13),
    labelLarge = texto(16, 600),
    labelMedium = texto(13, 600),
    labelSmall = texto(12, 600),
)
