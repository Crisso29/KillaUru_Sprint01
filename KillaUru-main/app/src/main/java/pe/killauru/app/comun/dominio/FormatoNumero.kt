package pe.killauru.app.comun.dominio

import java.math.RoundingMode
import java.text.DecimalFormat
import java.text.DecimalFormatSymbols

/**
 * Números como en el prototipo: coma decimal y punto de miles («1,2 km», «288,4», «2,3 MB»). Se fija a mano porque
 * Java usa punto decimal para `es-PE` y el texto no debe cambiar según el idioma del celular.
 */
internal object FormatoNumero {
    private val SIMBOLOS = DecimalFormatSymbols().apply {
        decimalSeparator = ','
        groupingSeparator = '.'
    }

    /** Redondea al medio hacia arriba y quita los ceros decimales sobrantes: 38,0 → «38», 2,25 → «2,3». */
    fun decimal(valor: Double, decimales: Int): String =
        DecimalFormat(if (decimales > 0) "#,##0." + "#".repeat(decimales) else "#,##0", SIMBOLOS)
            .apply { roundingMode = RoundingMode.HALF_UP }.format(valor)
}
