package pe.killauru.app.mapa.dominio

import pe.killauru.app.comun.dominio.Coordenada

/** FR-049, FR-051: polígono de la zona demo (máx 60 vértices). */
internal class ZonaDemo(val vertices: List<Coordenada>) {
    fun contiene(punto: Coordenada): Boolean {
        if (vertices.size < 3) return false
        var dentro = false
        var j = vertices.size - 1
        for (i in vertices.indices) {
            val vi = vertices[i]
            val vj = vertices[j]
            if ((vi.lat > punto.lat) != (vj.lat > punto.lat) &&
                punto.lon < (vj.lon - vi.lon) * (punto.lat - vi.lat) / (vj.lat - vi.lat) + vi.lon
            ) {
                dentro = !dentro
            }
            j = i
        }
        return dentro
    }
}
