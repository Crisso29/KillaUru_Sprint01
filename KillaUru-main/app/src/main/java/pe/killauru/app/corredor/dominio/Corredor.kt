package pe.killauru.app.corredor.dominio

import pe.killauru.app.comun.dominio.Coordenada
import kotlin.math.*

/** FR-042, FR-055: proyecta una coordenada sobre la polilínea del corredor. */
internal class Corredor(val vertices: List<Vertice>) {
    private val maxDistanciaM = 50.0
    private val radioTierraM = 6_371_008.8

    fun proyectar(punto: Coordenada, desdeSegmento: Int = 0): PosicionEnVia? {
        var mejorDist = Double.MAX_VALUE
        var mejorPos: PosicionEnVia? = null

        val inicio = maxOf(0, desdeSegmento)
        for (i in inicio until vertices.size - 1) {
            val a = vertices[i]; val b = vertices[i + 1]
            val pos = proyectarEnSegmento(punto, a, b, i) ?: continue
            val dist = haversine(punto.lat, punto.lon, pos.lat, pos.lon)
            if (dist < mejorDist) { mejorDist = dist; mejorPos = pos }
        }
        return if (mejorDist <= maxDistanciaM) mejorPos else null
    }

    private fun proyectarEnSegmento(p: Coordenada, a: Vertice, b: Vertice, seg: Int): PosicionEnVia? {
        val ax = a.lon; val ay = a.lat; val bx = b.lon; val by = b.lat
        val dx = bx - ax; val dy = by - ay
        val len2 = dx * dx + dy * dy
        if (len2 == 0.0) return null
        val t = ((p.lon - ax) * dx + (p.lat - ay) * dy) / len2
        val tc = t.coerceIn(0.0, 1.0)
        val px = ax + tc * dx; val py = ay + tc * dy
        val km = a.km + tc * (b.km - a.km)
        return PosicionEnVia(km, py, px, seg)
    }

    private fun haversine(lat1: Double, lon1: Double, lat2: Double, lon2: Double): Double {
        val dLat = Math.toRadians(lat2 - lat1); val dLon = Math.toRadians(lon2 - lon1)
        val a = sin(dLat / 2).pow(2) + cos(Math.toRadians(lat1)) * cos(Math.toRadians(lat2)) * sin(dLon / 2).pow(2)
        return radioTierraM * 2 * asin(sqrt(a))
    }
}
