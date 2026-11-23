package pe.killauru.app.mapa.ui

import org.maplibre.android.camera.CameraUpdateFactory
import org.maplibre.android.geometry.LatLng
import org.maplibre.android.geometry.LatLngBounds
import org.maplibre.android.maps.MapLibreMap
import org.maplibre.android.maps.Style
import org.maplibre.android.style.expressions.Expression
import org.maplibre.android.style.layers.CircleLayer
import org.maplibre.android.style.layers.LineLayer
import org.maplibre.android.style.layers.Property
import org.maplibre.android.style.layers.PropertyFactory
import org.maplibre.android.style.sources.GeoJsonSource
import org.maplibre.geojson.Feature
import org.maplibre.geojson.FeatureCollection
import org.maplibre.geojson.LineString
import org.maplibre.geojson.Point
import pe.killauru.app.mapa.dominio.AnomaliaLocal

/** Colores de la vía y de cada severidad (los tokens de `Paleta`, como enteros ARGB para MapLibre). */
internal class ColoresMapa(val via: Int, val bordeVia: Int, val contorno: Int, val severidades: List<Int>)

/**
 * FR-040, FR-049 (prototipo «Mapa»): lo que la app dibuja encima del mapa base — la PE-28A con su borde dorado y las
 * anomalías con el color de su severidad.
 */
internal object CapasMapa {
    const val CAPA_ANOMALIAS = "anomalias-puntos"
    private const val FUENTE_ANOMALIAS = "anomalias"
    private const val FUENTE_VIA = "via"
    private const val MARGEN_PX = 64

    fun agregarVia(estilo: Style, via: List<Pair<Double, Double>>, colores: ColoresMapa) {
        if (via.size < 2 || estilo.getSource(FUENTE_VIA) != null) return
        estilo.addSource(GeoJsonSource(FUENTE_VIA, LineString.fromLngLats(via.map { (lon, lat) -> Point.fromLngLat(lon, lat) })))
        estilo.addLayer(linea("via-borde", colores.bordeVia, ancho = 9f))
        estilo.addLayer(linea("via", colores.via, ancho = 5f))
    }

    private fun linea(id: String, color: Int, ancho: Float) = LineLayer(id, FUENTE_VIA).withProperties(
        PropertyFactory.lineColor(color),
        PropertyFactory.lineWidth(ancho),
        PropertyFactory.lineCap(Property.LINE_CAP_ROUND),
        PropertyFactory.lineJoin(Property.LINE_JOIN_ROUND),
    )

    /** Crea la capa la primera vez y después solo cambia los datos. */
    fun ponerAnomalias(estilo: Style, anomalias: List<AnomaliaLocal>, colores: ColoresMapa) {
        val datos = FeatureCollection.fromFeatures(anomalias.map(::feature))
        (estilo.getSource(FUENTE_ANOMALIAS) as? GeoJsonSource)?.setGeoJson(datos)?.let { return }
        estilo.addSource(GeoJsonSource(FUENTE_ANOMALIAS, datos))
        estilo.addLayer(CircleLayer(CAPA_ANOMALIAS, FUENTE_ANOMALIAS).withProperties(
            PropertyFactory.circleRadius(9f),
            PropertyFactory.circleColor(colorPorSeveridad(colores.severidades)),
            PropertyFactory.circleStrokeColor(colores.contorno),
            PropertyFactory.circleStrokeWidth(2f),
        ))
    }

    private fun colorPorSeveridad(colores: List<Int>): Expression = Expression.match(
        Expression.toNumber(Expression.get("severidad")),
        Expression.color(colores.last()),
        *colores.mapIndexed { i, c -> Expression.stop(i + 1, Expression.color(c)) }.toTypedArray(),
    )

    private fun feature(a: AnomaliaLocal) = Feature.fromGeometry(Point.fromLngLat(a.coordenada.longitud, a.coordenada.latitud)).apply {
        addStringProperty("id", a.id)
        addNumberProperty("severidad", a.severidad)
    }

    /** Al abrir, la cámara muestra la vía entera (aún sin GPS). */
    fun encuadrar(mapa: MapLibreMap, via: List<Pair<Double, Double>>) {
        if (via.size < 2) return
        val limites = LatLngBounds.Builder().includes(via.map { (lon, lat) -> LatLng(lat, lon) }).build()
        mapa.moveCamera(CameraUpdateFactory.newLatLngBounds(limites, MARGEN_PX))
    }
}
