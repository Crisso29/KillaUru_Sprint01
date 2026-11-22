package pe.killauru.app.comun.ui

import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.withStyle

/**
 * Convierte `**así**` en negrita. Los textos legales vienen del APK (`assets/legal/textos.json`) con esa marca mínima,
 * para que el redactor pueda resaltar palabras sin tocar el código. Un `**` sin cerrar se muestra tal cual.
 */
internal fun textoConNegritas(texto: String): AnnotatedString = buildAnnotatedString {
    val partes = texto.split("**")
    val cerrado = partes.size % 2 == 1
    partes.forEachIndexed { i, parte ->
        val enNegrita = i % 2 == 1 && (cerrado || i < partes.lastIndex)
        if (enNegrita) withStyle(SpanStyle(fontWeight = FontWeight.Bold)) { append(parte) }
        else append(if (i % 2 == 1) "**$parte" else parte)
    }
}
