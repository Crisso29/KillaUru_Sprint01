package pe.killauru.app.comun.dominio

import kotlin.math.PI
import kotlin.math.asin
import kotlin.math.cos
import kotlin.math.hypot
import kotlin.math.sin
import kotlin.math.sqrt

/** R-016 / FR-074: fórmulas idénticas a GeodesiaLocal del backend, sin red. */
internal object Geodesia {
    private const val RADIO_TIERRA_METROS = 6_371_008.8
    private const val RADIANES = PI / 180

    fun distanciaMetros(inicio: Coordenada, fin: Coordenada): Double {
        val lat = sin((fin.latitud - inicio.latitud) * RADIANES / 2)
        val lon = sin((fin.longitud - inicio.longitud) * RADIANES / 2)
        val h = lat * lat + cos(inicio.latitud * RADIANES) * cos(fin.latitud * RADIANES) * lon * lon
        return 2 * RADIO_TIERRA_METROS * asin(sqrt(h.coerceIn(0.0, 1.0)))
    }

    fun proyectar(punto: Coordenada, inicio: Coordenada, fin: Coordenada): ProyeccionSegmento {
        val escala = RADIO_TIERRA_METROS * RADIANES
        val coseno = cos((inicio.latitud + fin.latitud) * RADIANES / 2)
        val x = (fin.longitud - inicio.longitud) * escala * coseno
        val y = (fin.latitud - inicio.latitud) * escala
        val px = (punto.longitud - inicio.longitud) * escala * coseno
        val py = (punto.latitud - inicio.latitud) * escala
        val longitudCuadrada = x * x + y * y
        val fraccion = if (longitudCuadrada == 0.0) 0.0 else
            ((px * x + py * y) / longitudCuadrada).coerceIn(0.0, 1.0)
        return ProyeccionSegmento(hypot(px - fraccion * x, py - fraccion * y), fraccion)
    }
}
