package pe.killauru.app.comun.ui

// Generado por herramientas/marca/generar_iconos_android.py desde el prototipo: no editar a mano.
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.graphics.vector.PathParser
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

/**
 * Íconos del sistema de diseño (prototipo `ic()`, mismos trazos que el panel): línea de 1,8 sobre 24 × 24, redondeada.
 * Se tiñen con `Icon(tint = …)`, como cualquier ícono de Material.
 */
internal object IconosKillaUru {
    private val TRAZOS = mapOf(
        "map" to "M9 4 3 6v14l6-2 6 2 6-2V4l-6 2-6-2Z M9 4v14M15 6v14",
        "hist" to "M3 12a9 9 0 1 0 3-6.7 M3 4v5h5 M12 7v5l3 2",
        "gear" to "M9.0 12.0a3.0 3.0 0 1 0 6.0 0a3.0 3.0 0 1 0 -6.0 0Z M12 2v3M12 19v3M4.9 4.9l2.1 2.1M17 17l2.1 2.1M2 12h3M19 12h3M4.9 19.1 7 17M17 7l2.1-2.1",
        "user" to "M8.0 8.0a4.0 4.0 0 1 0 8.0 0a4.0 4.0 0 1 0 -8.0 0Z M4 21a8 8 0 0 1 16 0",
        "back" to "M15 18 9 12l6-6",
        "close" to "M18 6 6 18M6 6l12 12",
        "chev" to "m9 18 6-6-6-6",
        "pin" to "M12 21s-7-6.2-7-11a7 7 0 1 1 14 0c0 4.8-7 11-7 11Z M9.5 10.0a2.5 2.5 0 1 0 5.0 0a2.5 2.5 0 1 0 -5.0 0Z",
        "bell" to "M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9 M10.3 21a1.9 1.9 0 0 0 3.4 0",
        "battery" to "M4.0 7.0h14.0a2.0 2.0 0 0 1 2.0 2.0v6.0a2.0 2.0 0 0 1 -2.0 2.0h-14.0a2.0 2.0 0 0 1 -2.0 -2.0v-6.0a2.0 2.0 0 0 1 2.0 -2.0Z M22 11v2M6 10v4",
        "wifi" to "M5 12.6a10 10 0 0 1 14 0M8.5 16a5 5 0 0 1 7 0M2 9a15 15 0 0 1 20 0 M11.2 19.5a0.8 0.8 0 1 0 1.6 0a0.8 0.8 0 1 0 -1.6 0Z",
        "down" to "M12 4v12M6 11l6 6 6-6M4 20h16",
        "shield" to "M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z m9 12 2 2 4-4",
        "check" to "M20 6 9 17l-5-5",
        "warn" to "M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z M12 9v4M12 17h.01",
        "info" to "M3.0 12.0a9.0 9.0 0 1 0 18.0 0a9.0 9.0 0 1 0 -18.0 0Z M12 16v-4M12 8h.01",
        "target" to "M5.0 12.0a7.0 7.0 0 1 0 14.0 0a7.0 7.0 0 1 0 -14.0 0Z M9.5 12.0a2.5 2.5 0 1 0 5.0 0a2.5 2.5 0 1 0 -5.0 0Z M12 2v3M12 19v3M2 12h3M19 12h3",
        "layers" to "m12 3 9 5-9 5-9-5 9-5Z m3 13 9 5 9-5",
        "sound" to "M11 5 6 9H2v6h4l5 4V5Z M15.5 8.5a5 5 0 0 1 0 7M19 5a10 10 0 0 1 0 14",
        "mail" to "M5.0 5.0h14.0a2.0 2.0 0 0 1 2.0 2.0v10.0a2.0 2.0 0 0 1 -2.0 2.0h-14.0a2.0 2.0 0 0 1 -2.0 -2.0v-10.0a2.0 2.0 0 0 1 2.0 -2.0Z m3 7 9 6 9-6",
        "lock" to "M7.0 11.0h10.0a2.0 2.0 0 0 1 2.0 2.0v6.0a2.0 2.0 0 0 1 -2.0 2.0h-10.0a2.0 2.0 0 0 1 -2.0 -2.0v-6.0a2.0 2.0 0 0 1 2.0 -2.0Z M8 11V7a4 4 0 0 1 8 0v4",
        "phone" to "M9.0 2.0h6.0a2.0 2.0 0 0 1 2.0 2.0v16.0a2.0 2.0 0 0 1 -2.0 2.0h-6.0a2.0 2.0 0 0 1 -2.0 -2.0v-16.0a2.0 2.0 0 0 1 2.0 -2.0Z M11 18h2",
        "car" to "M5 17h14v-5l-2-5H7l-2 5v5Z M3 12h18 M5.7 17.0a1.8 1.8 0 1 0 3.6 0a1.8 1.8 0 1 0 -3.6 0Z M14.7 17.0a1.8 1.8 0 1 0 3.6 0a1.8 1.8 0 1 0 -3.6 0Z",
        "truck" to "M2 6h11v10H2z M13 9h4l4 4v3h-8 M4.2 17.5a1.8 1.8 0 1 0 3.6 0a1.8 1.8 0 1 0 -3.6 0Z M15.2 17.5a1.8 1.8 0 1 0 3.6 0a1.8 1.8 0 1 0 -3.6 0Z",
        "moto" to "M2.5 16.5a3.0 3.0 0 1 0 6.0 0a3.0 3.0 0 1 0 -6.0 0Z M15.5 16.5a3.0 3.0 0 1 0 6.0 0a3.0 3.0 0 1 0 -6.0 0Z m5.5 16.5 4-6h5l4 6M14.5 10.5 13 6h3",
        "clock" to "M3.0 12.0a9.0 9.0 0 1 0 18.0 0a9.0 9.0 0 1 0 -18.0 0Z M12 7v5l3 2",
        "refresh" to "M21 12a9 9 0 1 1-2.6-6.4L21 8 M21 3v5h-5",
        "ext" to "M15 3h6v6M10 14 21 3M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6",
        "out" to "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9",
        "compass" to "M3.0 12.0a9.0 9.0 0 1 0 18.0 0a9.0 9.0 0 1 0 -18.0 0Z m15.5 8.5-2 5-5 2 2-5 5-2Z",
        "plus" to "M12 5v14M5 12h14",
        "minus" to "M5 12h14",
    )

    private val cache = mutableMapOf<String, ImageVector>()

    /** El ícono por su nombre del prototipo («car», «shield»…); falla si no existe para detectarlo en pruebas. */
    fun de(nombre: String): ImageVector = cache.getOrPut(nombre) {
        val datos = requireNotNull(TRAZOS[nombre]) { "Ícono desconocido: $nombre" }
        ImageVector.Builder(nombre, 24.dp, 24.dp, 24f, 24f).addPath(
            pathData = PathParser().parsePathString(datos).toNodes(),
            fill = null,
            stroke = SolidColor(Color.Black),
            strokeLineWidth = 1.8f,
            strokeLineCap = StrokeCap.Round,
            strokeLineJoin = StrokeJoin.Round,
        ).build()
    }

    val nombres: Set<String> get() = TRAZOS.keys
}
