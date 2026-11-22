package pe.killauru.app.comun.ui.tema

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.ReadOnlyComposable
import androidx.compose.runtime.staticCompositionLocalOf

/** Paleta completa (severidades, ok, aviso…) para lo que Material no nombra. Se lee con `KillaUruTema.paleta`. */
private val LocalPaleta = staticCompositionLocalOf { Paleta.clara }

/**
 * T024: tema de KillaUru. Sigue al sistema en claro u oscuro, como el panel con `prefers-color-scheme`.
 * Los componentes de Material ya toman la marca; los colores propios se piden con [KillaUruTema.paleta].
 */
@Composable
internal fun KillaUruTema(oscuro: Boolean = isSystemInDarkTheme(), contenido: @Composable () -> Unit) {
    val paleta = if (oscuro) Paleta.oscura else Paleta.clara
    CompositionLocalProvider(LocalPaleta provides paleta) {
        MaterialTheme(colorScheme = paleta.esquemaMaterial(oscuro), typography = TipografiaKillaUru, content = contenido)
    }
}

internal object KillaUruTema {
    val paleta: Paleta
        @Composable @ReadOnlyComposable get() = LocalPaleta.current
}
